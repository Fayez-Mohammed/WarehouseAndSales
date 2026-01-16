using System.Collections.Generic;

namespace Base.API.DTOs
{
    public class DashboardStatsDto
    {
        public decimal TotalSalesToday { get; set; }
        public decimal TotalProfitToday { get; set; }
       // public int PendingOrdersCount { get; set; }
        public int ApprovedOrdersCountToday { get; set; }
        public List<LowStockProductDto> LowStockProducts { get; set; }
    }

    public class LowStockProductDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int CurrentQuantity { get; set; }
        public string SKU { get; set; }
        public int ProductCode { get; internal set; }
    }
}