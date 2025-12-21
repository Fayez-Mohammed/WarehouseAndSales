using Base.API.DTOs;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Accountant,SystemAdmin,StoreManager")] // الصلاحيات: محاسب، أدمن، مدير مخزن
    public class ReportsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// تقرير المبيعات خلال فترة زمنية محددة
        /// </summary>
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            
            var start = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var end = toDate ?? DateTime.UtcNow;

            var spec = new BaseSpecification<Order>(o =>
                o.Status == OrderStatus.Approved &&
                o.ApprovedDate >= start &&
                o.ApprovedDate <= end);

            var orders = await _unitOfWork.Repository<Order>().ListAsync(spec);

            var report = new SalesReportDto
            {
                FromDate = start,
                ToDate = end,
                TotalOrders = orders.Count,
                TotalSales = orders.Sum(o => o.TotalAmount)
            };

            return Ok(report);
        }

        /// <summary>
        /// تقرير حركة صنف معين (Stock Movement)
        /// </summary>
        [HttpGet("stock-movement")]
        public async Task<IActionResult> GetStockMovementReport([FromQuery]string productId)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null) return NotFound("Product not found");

            
            var spec = new BaseSpecification<StockTransaction>(t => t.ProductId == productId);
            var transactions = await _unitOfWork.Repository<StockTransaction>().ListAsync(spec);

            var report = new StockMovementReportDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CurrentStock = product.CurrentStockQuantity,
               
                TotalIn = transactions
                            .Where(t => t.Type == TransactionType.StockIn || (t.Type == TransactionType.Adjustment && t.Quantity > 0))
                            .Sum(t => Math.Abs(t.Quantity)),
             
                TotalOut = transactions
                            .Where(t => t.Type == TransactionType.StockOut || (t.Type == TransactionType.Adjustment && t.Quantity < 0))
                            .Sum(t => Math.Abs(t.Quantity))
            };

            return Ok(report);
        }

        /// <summary>
        /// تقرير عمولات المندوبين (Commissions)
        /// </summary>
        [HttpGet("commissions")]
        public async Task<IActionResult> GetCommissionsReport([FromQuery] string? salesRepId)
        {
 
            var spec = new BaseSpecification<Invoice>(i => i.Type == InvoiceType.CommissionInvoice);

         
            spec.Includes.Add(i => i.Order);
            spec.Includes.Add(i => i.Order.SalesRep); 

            var invoices = await _unitOfWork.Repository<Invoice>().ListAsync(spec);


            if (!string.IsNullOrEmpty(salesRepId))
            {
                invoices = invoices.Where(i => i.Order.SalesRepId == salesRepId ).ToList();
            }


            var report = invoices
                .GroupBy(i => i.Order.SalesRepId)
                .Select(g => new CommissionReportDto
                {
                    SalesRepId = g.Key,
                    SalesRepName = g.First().Order.SalesRep?.FullName ?? g.First().RecipientName,
                    TotalCommission = g.Sum(i => i.Amount),
                    TotalOrdersConfirmed = g.Count()
                })
                .ToList();

            return Ok(report);
        }
    }
}