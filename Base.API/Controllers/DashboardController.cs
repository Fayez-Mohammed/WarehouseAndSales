using Base.API.DTOs;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Required for Include/ThenInclude logic
using RepositoryProject.Specifications;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin,Accountant,StoreManager")]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int LowStockThreshold = 10;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var response = new DashboardStatsDto();
            var today = DateTime.UtcNow.Date;

            // =========================================================
            // 1. Total Sales & Profit (Using Optimized ThenInclude)
            // =========================================================

            var salesSpec = new BaseSpecification<Order>(o =>
                o.Status == OrderStatus.Approved &&
                o.ApprovedDate >= today);

            // This fetches Order -> Items -> Product in a SINGLE query
            salesSpec.AllIncludes.Add(query => query
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product));

            var todayOrders = await _unitOfWork.Repository<Order>().ListAsync(salesSpec);

            response.TotalSalesToday = 0;
            response.TotalProfitToday = 0;

            foreach (var order in todayOrders)
            {
                response.TotalSalesToday += order.TotalAmount;

                foreach (var item in order.OrderItems)
                {
                    // Calculation:
                    // Revenue = UnitPrice (sold at) * Qty
                    // Cost    = Product.BuyPrice (current cost) * Qty
                    if (item.Product != null)
                    {
                        var revenue = item.UnitPrice * item.Quantity;
                        var cost = item.Product.BuyPrice * item.Quantity;
                        response.TotalProfitToday += (revenue - cost);
                    }
                }
            }

            // =========================================================
            // 2. Pending Orders Count
            // =========================================================
            var pendingSpec = new BaseSpecification<Order>(o => o.Status == OrderStatus.Confirmed);
            response.PendingOrdersCount = await _unitOfWork.Repository<Order>().CountAsync(pendingSpec);
            // =========================================================
            // 2.1. Approved Orders Count
            // =========================================================
            var ApproveSpec = new BaseSpecification<Order>(o => o.Status == OrderStatus.Approved &&
                o.ApprovedDate >= today);
            response.ApprovedOrdersCountToday = await _unitOfWork.Repository<Order>().CountAsync(ApproveSpec);
            // =========================================================
            // 3. Low Stock Products
            // =========================================================
            var lowStockSpec = new BaseSpecification<Product>(p =>
                !p.IsDeleted &&
                p.CurrentStockQuantity <= LowStockThreshold);

            lowStockSpec.AddOrderBy(p => p.CurrentStockQuantity);
            lowStockSpec.ApplyPaging(0, 10); // Get top 10 most critical

            var lowStockProducts = await _unitOfWork.Repository<Product>().ListAsync(lowStockSpec);

            response.LowStockProducts = lowStockProducts.Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                SKU = p.SKU,
                CurrentQuantity = p.CurrentStockQuantity
            }).ToList();

            return Ok(response);
        }
    }
}