using System;

namespace Base.API.DTOs
{
    public class SalesReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
    }

    public class StockMovementReportDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalIn { get; set; }
        public int TotalInPurchased { get; set; }
        public int TotalInReturned { get; set; }
        public int TotalInAdjusted { get; set; }
        public int TotalInUpdatedByEmployee { get; set; }
        public int TotalOut { get; set; }
        public int TotalOutSold { get; set; }
        public int TotalOutAdjusted { get; set; }
        public int TotalOutUpdatedByEmployee { get; set; }

        public int CurrentStock { get; set; }
    }

    public class CommissionReportDto
    {
        public string SalesRepId { get; set; }
        public string SalesRepName { get; set; }
        public decimal TotalCommission { get; set; }
        public int TotalOrdersConfirmed { get; set; }
    }
}