using Base.DAL.Models.SystemModels.Enums;

namespace Base.API.DTOs
{
    internal class ApprovedOrderDto
    {
        public string Id { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string CustomerName { get; set; }
        public string SalesRepName { get; set; }
        public DateTime DateOfCreation { get; set; }
    }
}