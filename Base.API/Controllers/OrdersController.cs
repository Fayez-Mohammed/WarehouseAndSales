using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;
using System.Security.Claims;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public OrdersController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }
        /// <summary>
        /// Gets all approved orders for the logged-in customer.   Not Used Currently
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllApprovedOrdersForCustomer")]
      //  [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAllApprovedOrdersForCustomer()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var repo= unitOfWork.Repository<Order>();
            var spec = new BaseSpecification<Order>(o => o.Status == OrderStatus.Approved && o.CustomerId == customerId);
            var approvedOrders = await repo.ListAsync(spec);
            var approvedOrdersDto = approvedOrders.Select(o => new
            {
                o.Id,
                o.TotalAmount,
               // o.CommissionAmount,
                o.Status,
                o.CustomerId,
                o.SalesRepId,
                o.DateOfCreation
            });
            return Ok(approvedOrdersDto);
        }
        /// <summary>
        ///  Not Used Currently
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllPendingOrdersForSalesRepoToConfirmLater")]
      //  [Authorize(Roles = "SalesRep")]
        public async Task<IActionResult> GetAllPendingOrdersForSalesRepoToConfirmLater()
        {
            var salesRepId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var repo= unitOfWork.Repository<Order>();
            var spec = new BaseSpecification<Order>(o => o.Status == OrderStatus.Pending && o.SalesRepId == salesRepId);

            var pendingOrders = await repo.ListAsync(spec);
            var pendingOrdersDto = pendingOrders.Select(o => new
            {
                o.Id,
                o.TotalAmount,
                o.CommissionAmount,
                o.Status,
                o.CustomerId,
                o.SalesRepId,
                o.DateOfCreation
            });
            return Ok(pendingOrdersDto);
        }

        /// <summary>
        /// Creates a new order for the logged-in customer.  Not Used Currently
        /// Status defaults to 'Pending'.
        /// </summary>
        /// <param name="dto">Order details including items and optional SalesRepId.</param>
        /// <returns>The created Order ID.</returns>
        [HttpPost]
     //   [Authorize(Roles = "Customer")] // Ensure only Customers can place orders
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Get Logged-in User ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. Prepare Order Items & Calculate Total
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();
            var productRepo = unitOfWork.Repository<Product>();

            foreach (var itemDto in dto.Items)
            {
                var product = await productRepo.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    return BadRequest($"Product with ID {itemDto.ProductId} not found.");

                // Optional: Check stock availability (don't deduct yet, just validate)
                if (product.CurrentStockQuantity < itemDto.Quantity)
                {
                    return BadRequest($"Insufficient stock for product '{product.Name}'. Available: {product.CurrentStockQuantity}");
                }

                // Calculate price for this line item
                decimal itemTotal = product.SellPrice * itemDto.Quantity;
                totalAmount += itemTotal;

                // Create OrderItem (Price Snapshot)
                orderItems.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.SellPrice // Capture price at moment of order
                });
            }

            // 3. Create the Order Entity
            var order = new Order
            {
                CustomerId = userId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending, // Default Status
                OrderItems = orderItems,      // EF Core will insert these automatically
                SalesRepId = dto.SalesRepId,   // Optional, can be null
                CommissionAmount=0.1m*totalAmount
             };

            // 4. Save to Database
            await unitOfWork.Repository<Order>().AddAsync(order);
            var result = await unitOfWork.CompleteAsync();

            if (result <= 0)
                return StatusCode(500, "Failed to create order.");

            return Ok(new { Message = "Order created successfully", OrderId = order.Id });
        }



        /// <summary>
        /// تاكيد الطلب وخصم المخزون   
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPut("ApproveOrderByStoreManager")]
        [Authorize(Roles = "StoreManager")]
        public async Task<IActionResult> ApproveOrder([FromQuery] string orderId)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Get Order with Includes
            var spec = new BaseSpecification<Order>(o => o.Id == orderId);
            spec.Includes.Add(o => o.OrderItems);
            spec.Includes.Add(o => o.Customer);

            var order = await unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);

            if (order == null) return NotFound("Order not found");

            if (order.Status != OrderStatus.Confirmed)
                return BadRequest("Order must be Confirmed by Sales Rep first.");

            // 2. Process Items & Deduct Stock
            foreach (var item in order.OrderItems)
            {
                // Retrieve Product explicitly
                var product = await unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);

                if (product.CurrentStockQuantity < item.Quantity)
                {
                    return BadRequest($"Insufficient stock for product '{product.Name}'");
                }

                // Deduct Stock
                product.CurrentStockQuantity -= item.Quantity;
                await unitOfWork.Repository<Product>().UpdateAsync(product);

                // Create Stock Transaction Log
                var stockLog = new StockTransaction
                {
                    ProductId = item.ProductId,
                    OrderId = order.Id,
                    StoreManagerId = managerId,
                    Type = TransactionType.StockOut,
                    Quantity = -item.Quantity
                    // REMOVED: TransactionDate = DateTime.UtcNow 
                    // Your AppDbContext automatically fills 'DateOfCreation' which serves as the date.
                };
                await unitOfWork.Repository<StockTransaction>().AddAsync(stockLog);
            }

            // 3. Generate Customer Invoice
            var custInvoice = new Invoice
            {
                OrderId = order.Id,
                Type = InvoiceType.CustomerInvoice,
                RecipientName = order.Customer.FullName,
                Amount = order.TotalAmount,
                RemainingAmount = order.TotalAmount,
                DateOfCreation = DateTime.UtcNow
            };
            await unitOfWork.Repository<Invoice>().AddAsync(custInvoice);

            // 4. Generate Commission Invoice (if Sales Rep exists)
            if (order.SalesRepId != null)
            {
                var salesRep = await userManager.FindByIdAsync(order.SalesRepId);

                var commInvoice = new Invoice
                {
                    OrderId = order.Id,
                    Type = InvoiceType.CommissionInvoice,
                    RecipientName = salesRep?.FullName ?? "Sales Rep",
                    Amount = order.CommissionAmount,
                    RemainingAmount = order.CommissionAmount,
                    DateOfCreation = DateTime.UtcNow
                };
                await unitOfWork.Repository<Invoice>().AddAsync(commInvoice);
            }

            // 5. Finalize Order
            order.Status = OrderStatus.Approved;
            order.ApprovedDate = DateTime.UtcNow;
            await unitOfWork.Repository<Order>().UpdateAsync(order);

            // 6. Commit all changes
            var result = await unitOfWork.CompleteAsync();

            if (result <= 0) return StatusCode(500, "Failed to approve order");

            return Ok(new { Message = "Order Approved and Stock Deducted" });
        }
        /// <summary>
        ///  Not Used Currently
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPut("Approve_Order_By_SalesRep")]
        [Authorize(Roles = "SalesRep")]
        public async Task<IActionResult> ConfirmOrder([FromQuery] string orderId)
        {
            var salesRepId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // 1. Get Order
            var order = await unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return NotFound("Order not found");
            if (order.Status != OrderStatus.Pending)
                return BadRequest("Only Pending orders can be confirmed.");
            // 2. Calculate Commission (e.g., 10% of TotalAmount)

            decimal commissionRate = 0.10m; // 10%
            order.CommissionAmount = order.TotalAmount * commissionRate;
            order.Status = OrderStatus.Confirmed;
            order.SalesRepId = salesRepId;
            await unitOfWork.Repository<Order>().UpdateAsync(order);
            var result = await unitOfWork.CompleteAsync();
            if (result <= 0)
                return StatusCode(500, "Failed to confirm order.");
            return Ok(new { Message = $"Order With ID= [{order.Id}] confirmed by Sales Rep", CommissionAmount = order.CommissionAmount });
        }
        //////////////////////////
        /// <summary>
        /// انشاء طلب وتأكيده مباشرة من قبل مدير المتجر نيابة عن عميل معين
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("Create_Order_By_Store_Manager_By_Customer_Id_And_SalesRepId")]
        [Authorize(Roles = "StoreManager")]
        public async Task<IActionResult> CreateOrderByStoreManagerByCustomerIdAndSalesRepId([FromBody] CreateOrderByManagerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // 1. Validate Customer
            var customer = await userManager.FindByIdAsync(dto.CustomerId);
            if (customer == null || !await userManager.IsInRoleAsync(customer, "Customer"))
                return BadRequest("Invalid Customer ID.");
            // 2. Prepare Order Items & Calculate Total
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();
            var productRepo = unitOfWork.Repository<Product>();
            foreach (var itemDto in dto.Items)
            {
                var product = await productRepo.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    return BadRequest($"Product with ID {itemDto.ProductId} not found.");
                // Calculate price for this line item
                decimal itemTotal = product.SellPrice * itemDto.Quantity;
                totalAmount += itemTotal;
                // Create OrderItem (Price Snapshot)
                orderItems.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.SellPrice // Capture price at moment of order
                });
            }
            // 3. Create the Order Entity
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Confirmed, // Directly Confirmed
                OrderItems = orderItems,      // EF Core will insert these automatically
                SalesRepId = dto.SalesRepId,   // Optional, can be null
                CommissionAmount = 0.1m * totalAmount
            };
            // 4. Save to Database
            await unitOfWork.Repository<Order>().AddAsync(order);
            var result = await unitOfWork.CompleteAsync();
            if (result <= 0)
                return StatusCode(500, "Failed to create order.");
            return Ok(new { Message = "Order created and confirmed successfully", OrderId = order.Id });
        }
        /// <summary>
        /// جلب جميع الطلبات المؤكدة من قبل مندوبي المبيعات لمدير المتجر
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllConfirmedOrders")]
        [Authorize(Roles = "StoreManager")]
        public async Task<IActionResult> GetAllConfirmedOrders()
        {
            var repo = unitOfWork.Repository<Order>();
            var spec = new BaseSpecification<Order>(o => o.Status == OrderStatus.Confirmed);
            var confirmedOrders = await repo.ListAsync(spec);
            var confirmedOrdersDto = confirmedOrders.Select(o => new
            {
                o.Id,
                o.TotalAmount,
                o.CommissionAmount,
                o.Status,
                o.CustomerId,
                o.SalesRepId,
                o.DateOfCreation
            });
            return Ok(confirmedOrdersDto);
        }

        /// <summary>
        /// جلب جميع الطلبات المعتمدة من قبل مدير المتجر
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllApprovedOrders")]
        [Authorize(Roles = "StoreManager")]
        public async Task<IActionResult> GetAllApprovedOrders()
        {
            var repo = unitOfWork.Repository<Order>();
            var spec = new BaseSpecification<Order>(o => o.Status == OrderStatus.Approved);
            var approvedOrders = await repo.ListAsync(spec);
            var approvedOrdersDto = approvedOrders.Select(o => new
            {
                o.Id,
                o.TotalAmount,
                o.CommissionAmount,
                o.Status,
                o.CustomerId,
                o.SalesRepId,
                o.DateOfCreation
            });
            return Ok(approvedOrdersDto);
        }
    }
}




