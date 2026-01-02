using Base.API.DTOs;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
using Base.Shared.Responses; // Assuming you have ApiResponseDTO here
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RepositoryProject.Specifications;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin,Accountant,StoreManager")]// Only Store Managers should perform inventory checks
    public class InventoryCheckController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InventoryCheckController> _logger;

        public InventoryCheckController(IUnitOfWork unitOfWork, ILogger<InventoryCheckController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// جرد المخزون وتعديل الكميات بناءً على الفحص الفعلي.
        /// </summary>
        /// <param name="dto">The adjustment details including Product ID and Actual Quantity.</param>
        /// <param name="UpdateStock">Indicates whether to update the stock quantity in the system.</param>
        /// <returns>The result of the adjustment operation, including loss/profit details.</returns>
        [HttpPost("Adjust")]
        //[Authorize(Roles = "StoreManager")] // Only Store Managers can adjust stock after inventory check
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentDto dto,[FromQuery]bool UpdateStock)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { Message = "Invalid input data." });
            }

            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                //var productRepo = _unitOfWork.Repository<Product>();
                //var product = await productRepo.GetByIdAsync(dto.ProductId);
                var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(new BaseSpecification<Product>(p => p.Name == dto.ProductName));

                if (product == null)
                {
                    return NotFound(new ApiResponseDTO { Message = $"Product with Name {dto.ProductName} not found." });
                }

                int systemQuantity = product.CurrentStockQuantity;
                int difference = dto.ActualQuantity - systemQuantity;

                if (difference == 0)
                {
                    return Ok(new { Message = "No adjustment needed. System quantity matches physical count." });
                }
                var transactionType = TransactionType.Adjustment;
                decimal financialImpact = difference * product.BuyPrice;

                string impactDescription = difference > 0
                    ? $"Profit (Surplus): Found {difference} extra units. Value gain: {financialImpact:C}"
                    : $"Loss (Deficit): Missing {Math.Abs(difference)} units. Value loss: {Math.Abs(financialImpact):C}";

                // Update the product's stock to match the actual physical count
                product.CurrentStockQuantity = dto.ActualQuantity;


                var adjustmentTransaction = new StockTransaction
                {
                    ProductId = product.Id,
                    StoreManagerId = managerId,
                    Quantity = difference, // Positive for Surplus, Negative for Deficit
                    Type = transactionType,
                    DateOfCreation = DateTime.UtcNow,
                    Notes=impactDescription,
                    UnitBuyPrice = product.BuyPrice,
                    UnitSellPrice= product.SellPrice

                };
                if (UpdateStock)
                {
                    await _unitOfWork.Repository<StockTransaction>().AddAsync(adjustmentTransaction);

                    await _unitOfWork.Repository<Product>().UpdateAsync(product);

                    var result = await _unitOfWork.CompleteAsync();

                    if (result <= 0)
                    {
                        return StatusCode(500, new ApiResponseDTO { Message = "Failed to save stock adjustment." });
                    }

                    string statusMsg2 = difference > 0 ? "Surplus (Found)" : "Deficit (Missing)";

                    return Ok(new
                    {
                        Message = $"Stock adjusted successfully. {statusMsg2}: {Math.Abs(difference)} units.",
                        OldQuantity = systemQuantity,
                        NewQuantity = product.CurrentStockQuantity,
                        AdjustmentId = adjustmentTransaction.Id,
                        FinancialImpact = impactDescription,
                        ValueDifference = financialImpact
                    });

                }
                string statusMsg = difference > 0 ? "Surplus (Found)" : "Deficit (Missing)";

                return Ok(new
                {
                    Message = $" {statusMsg}: {Math.Abs(difference)} units.",
                    SystemQuantity = systemQuantity,
                    FinancialImpact = impactDescription,
                    ValueDifference = financialImpact
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting stock for product {ProductName}", dto.ProductName);
                return StatusCode(500, new ApiResponseDTO { Message = "Error processing stock adjustment." });
            }
        }
    }
}


