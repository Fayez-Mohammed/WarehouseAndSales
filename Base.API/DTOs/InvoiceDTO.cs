using Base.DAL.Models.SystemModels.Enums;

namespace Base.API.DTOs
{
    internal class InvoiceDTO
    {
        public string Id { get; set; }
        public InvoiceType Type { get; set; }
        public string RecipientName { get; set; }
        public decimal Amount { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string OrderId { get; set; }
    }
}