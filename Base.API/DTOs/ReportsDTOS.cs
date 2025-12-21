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
        public int TotalOut { get; set; }
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