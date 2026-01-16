using Base.DAL.Models.SystemModels.Enums;

namespace Base.API.DTOs
{
    public class InvoiceDTO
    {
        public string Id { get; set; }
        public int Code { get; set; }
        public InvoiceType Type { get; set; }
        public string RecipientName { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; } = 0m;  // الفلوس اللي العميل دفعها
        public decimal RemainingAmount { get; set; } = 0m;  // المديونية أو الباقي
        public DateTime GeneratedDate { get; set; }
        public string OrderId { get; set; }
        public int OrderCode { get; set; }
    }
    public class SupplierInvoiceDTO
    {
        public string Id { get; set; }
        public int Code { get; set; }
        public InvoiceType Type { get; set; }
        public string SupplierName { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; } = 0m;  // الفلوس اللي العميل دفعها
        public decimal RemainingAmount { get; set; } = 0m;  // المديونية أو الباقي
        public DateTime GeneratedDate { get; set; }
        public string SupplierId { get; set; }
    }
    public class InvoicePaidDTO
    {
        public decimal PaidAmount { get; set; } = 0m;  // الفلوس اللي العميل دفعها


    }
}