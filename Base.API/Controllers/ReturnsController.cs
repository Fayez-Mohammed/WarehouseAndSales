using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
using Base.Shared.Enums;
using Base.Shared.Responses;
using Hangfire.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryProject.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static ReturnRequest;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin,Accountant,StoreManager")]
    public class ReturnsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        public ReturnsController(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            this.userManager = userManager;

        }

        [HttpGet("OrderItemsByOrderId")]
        public IActionResult OrderItemsByOrderId([FromQuery] int orderCode)
        {
            var repo = _unitOfWork.Repository<Order>();
            var spec = new BaseSpecification<Order>(o => o.Code == orderCode);
            //spec.Includes.Add(o => o.OrderItems);
            spec.AllIncludes.Add(Q=> Q.Include(o => o.OrderItems).ThenInclude(oi => oi.Product));
          // spec.AddOrderBy(o=>o.OrderItems.Select(oi=> oi.Product.Name));
            var order = repo.GetEntityWithSpecAsync(spec).Result;
            if (order == null)
            {
                return NotFound("Order not found");
            }
            //var items = order.OrderItems.Select(oi => new
            //{
            //    oi.ProductId,
            //    oi.Quantity,
            //    oi.UnitPrice
                
            //}).ToList();
         var itemsDto= order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                Code = oi.Product.Code,
             ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                CustomerId= order.CustomerId
         }).ToList();
          
            return Ok(itemsDto);

        }
            /// <summary>
            /// Create a return request (Customer)
            /// </summary>
            [HttpPost]
     
        public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

           

            // 1. Validate Order ownership & status
            // We need to fetch order items to verify quantities
            var orderSpec = new BaseSpecification<Order>(o => o.Code == dto.OrderCode);
            orderSpec.AllIncludes.Add(Q => Q.Include(o => o.OrderItems).ThenInclude(oi => oi.Product));

            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(orderSpec);

            if (order == null) return NotFound("Order not found.");
            //if (order.CustomerId != dto.CustomerId) return Forbid();
            if (order.Status != OrderStatus.Approved)
                return BadRequest("Can only return items from Approved (Delivered) orders.");

            // 2. Validate Items & Quantities
            var returnItems = new List<ReturnItem>();
            foreach (var itemDto in dto.Items)
            {
                // Check if this product was actually in the order
                var originalItem = order.OrderItems.FirstOrDefault(oi => oi.Product.Name == itemDto.productName);
                if (originalItem == null)
                    return BadRequest($"Product {itemDto.productName} was not found in this order.");

                if (itemDto.Quantity > originalItem.Quantity)
                    return BadRequest($"Cannot return {itemDto.Quantity} of product {itemDto.productName}. Only {originalItem.Quantity} purchased.");

                // Note: Ideally, check previously returned quantity to prevent double returns.
                // Skipping for V1 simplicity.

                returnItems.Add(new ReturnItem
                {
                    ProductId = originalItem.ProductId,
                    Quantity = itemDto.Quantity,
                    Reason = itemDto.Reason
                });
            }

            // 3. Create Request
            var returnRequest = new ReturnRequest
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                Status = ReturnStatus.Pending,
                ReturnItems = returnItems
            };

            await _unitOfWork.Repository<ReturnRequest>().AddAsync(returnRequest);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return StatusCode(500, "Failed to create return request.");

           await ApproveReturnRequest(returnRequest.Id);
          //  return Ok(new { Message = "Return request submitted successfully.", RequestId = returnRequest.Id });
            return Ok(new { Message = "Return approved and items restocked.", RequestId = returnRequest.Id });
        }

        /// <summary>
        /// Get pending return requests (Store Manager)
        /// </summary>
        [HttpGet("pending")]
     
        public async Task<IActionResult> GetPendingReturns()
        {
            var spec = new BaseSpecification<ReturnRequest>(r => r.Status == ReturnStatus.Pending);
            spec.Includes.Add(r => r.Customer);
            // Note: Nested include for Items might require string include or additional loading logic
            // spec.Includes.Add("ReturnItems.Product"); 

            var requests = await _unitOfWork.Repository<ReturnRequest>().ListAsync(spec);
            var response = requests.Select(r => new
            {
                r.Id,
                r.Code,
                r.OrderId,
                CustomerName = r.Customer.FullName,
                r.Status,
                Items = r.ReturnItems.Select(ri => new
                {
                    ri.ProductId,
                    ri.Code,
                    ri.Quantity,
                    ri.Reason
                })
            });
            return Ok(response);
        }

        /// <summary>
        /// Approve return request and restock items (Store Manager)
        /// </summary>
        [HttpPut("approve")]
        [Authorize(Roles = "StoreManager")]
        public async Task<IActionResult> ApproveReturnRequest([FromQuery]string id)
        {
            var totalReturnedAmount = 0m;
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Get Request with Items
            var spec = new BaseSpecification<ReturnRequest>(r => r.Id == id);
            spec.Includes.Add(r => r.ReturnItems);

            var request = await _unitOfWork.Repository<ReturnRequest>().GetEntityWithSpecAsync(spec);

            if (request == null) return NotFound("Return request not found.");
            if (request.Status != ReturnStatus.Pending) return BadRequest("Request is not pending.");

            // 2. Process Stock Return
            foreach (var item in request.ReturnItems)
            {


                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    totalReturnedAmount += product.SellPrice * item.Quantity;
                    // Increase Stock
                    product.CurrentStockQuantity += item.Quantity;
                    await _unitOfWork.Repository<Product>().UpdateAsync(product);

                    // Log Transaction
                    var stockLog = new StockTransaction
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Type = TransactionType.Return, // Ensure this enum exists
                        StoreManagerId = managerId,
                        OrderId = request.OrderId, // Link to original order
                        DateOfCreation = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<StockTransaction>().AddAsync(stockLog);
                }
            }
            
            // 3. Update Request Status
            request.Status = ReturnStatus.Approved;
            await _unitOfWork.Repository<ReturnRequest>().UpdateAsync(request);


            var repoi =  _unitOfWork.Repository<Invoice>();
            var speci = new BaseSpecification<Invoice>(i => i.OrderId == request.OrderId && i.Type == InvoiceType.CustomerInvoice);
            var invoice = await repoi.GetEntityWithSpecAsync(speci);

            var returnInvoice = new Invoice
           {
               OrderId = request.OrderId,
               Type = InvoiceType.ReturnInvoice,
               RecipientName = invoice.RecipientName,
               Amount = totalReturnedAmount,
               RemainingAmount = totalReturnedAmount,
               DateOfCreation = DateTime.UtcNow
           };
              await _unitOfWork.Repository<Invoice>().AddAsync(returnInvoice);
            // 4. (Optional) Create Credit Note Invoice for Customer here

            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return StatusCode(500, "Failed to approve return.");

            return Ok(new { Message = "Return approved and items restocked." });
        }


        [HttpPost("returntosupplier")]
        public async Task<IActionResult> ReturnToSupplier([FromBody] ReturnToSupplierRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var query2 = userManager.Users
.Where(u => u.FullName.Contains(dto.SupplierName) && u.Type == UserTypes.Supplier);

            var count2 = await query2.CountAsync();

            if (count2 == 0)
                return BadRequest("Supplier not found");

            if (count2 > 1)
                return BadRequest("Multiple Suppliers found with the same name");

            var supplier = await query2.FirstAsync();
            if (supplier == null || !await userManager.IsInRoleAsync(supplier, "Supplier"))
                return BadRequest("Invalid Supplier Name.");


            var supplierRepo = _unitOfWork.Repository<Supplier>();
            var specSupplier = new BaseSpecification<Supplier>(s => s.UserId == supplier.Id);
            var supplierEntity = await supplierRepo.GetEntityWithSpecAsync(specSupplier);
            if(supplierEntity == null) return BadRequest("Invalid Supplier.");
            decimal totalReturnAmount = 0;
            var productRepo = _unitOfWork.Repository<Product>();
            var stockTransactionRepo = _unitOfWork.Repository<StockTransaction>();

  

            foreach (var item in dto.Items)
            {
               
                var spec = new BaseSpecification<Product>(p => p.Name == item.ProductName && !p.IsDeleted);
                var product = await productRepo.GetEntityWithSpecAsync(spec);

                if (product == null)
                    return BadRequest($"Product '{item.ProductName}' not found.");

               if(product.SupplierId!=supplierEntity.Id)
                   return BadRequest($"Product '{item.ProductName}' does not belong to the specified supplier.");

                if (product.CurrentStockQuantity < item.Quantity)
                {
                    return BadRequest($"Insufficient stock for '{product.Name}'. Current: {product.CurrentStockQuantity}, Return Requested: {item.Quantity}");
                }

               
                product.CurrentStockQuantity -= item.Quantity;
                await productRepo.UpdateAsync(product);

                
                var transaction = new StockTransaction
                {
                    ProductId = product.Id,
                    SupplierId = supplierEntity.Id,
                    StoreManagerId = User.FindFirstValue(ClaimTypes.NameIdentifier), 
                    Type = TransactionType.ReturnToSupplier, 
                    Quantity = -item.Quantity,
                    UnitBuyPrice = product.BuyPrice,
                    UnitSellPrice = product.SellPrice,
                    Notes = $"Return to Supplier: {item.Reason}",
                    DateOfCreation = DateTime.UtcNow
                };
                await stockTransactionRepo.AddAsync(transaction);

                
                totalReturnAmount += (product.BuyPrice * item.Quantity);
            }


            var returnInvoice = new SupplierInvoice
            {
                SupplierId = supplierEntity.Id,
                SupplierName = supplierEntity.Name,
                Type = InvoiceType.SupplierReturnInvoice, 
                Amount = totalReturnAmount, 
                RemainingAmount = totalReturnAmount, 
                PaidAmount = 0,
                DateOfCreation = DateTime.UtcNow
            };

            await _unitOfWork.Repository<SupplierInvoice>().AddAsync(returnInvoice);
            try
            {
                // 7. Save All Changes (Atomic Transaction)
                var result = await _unitOfWork.CompleteAsync();

                if (result <= 0) return StatusCode(500, "Failed to process return.");
            }
            catch (Exception ex) {
                return StatusCode(500, $"Error processing return: {ex.Message}");
            }
            return Ok(new
            {
                Message = "Return processed successfully",
                TotalValueRefunded = totalReturnAmount,
                NewInvoiceCode = returnInvoice.Code
            });
        }
    }
}

