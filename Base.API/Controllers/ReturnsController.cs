using Base.API.DTOs;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
using Base.Shared.Responses;
using Hangfire.Server;
using Microsoft.AspNetCore.Authorization;
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

        public ReturnsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("OrderItemsByOrderId")]
        public IActionResult OrderItemsByOrderId([FromQuery] string orderId)
        {
            var repo = _unitOfWork.Repository<Order>();
            var spec = new BaseSpecification<Order>(o => o.Id == orderId);
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
            var orderSpec = new BaseSpecification<Order>(o => o.Id == dto.OrderId);
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
                OrderId = dto.OrderId,
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
                r.OrderId,
                CustomerName = r.Customer.FullName,
                r.Status,
                Items = r.ReturnItems.Select(ri => new
                {
                    ri.ProductId,
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
    }
}