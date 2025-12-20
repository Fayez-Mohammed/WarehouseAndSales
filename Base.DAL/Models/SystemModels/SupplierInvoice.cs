using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.DAL.Models.SystemModels
{
    public class SupplierInvoice : BaseEntity
    {
        public InvoiceType Type { get; set; }

        // The person receiving the invoice (Customer OR SalesRep)
        public string SupplierName { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; } = 0m;  // الفلوس اللي العميل دفعها
        public decimal RemainingAmount { get; set; } = 0m;  // المديونية أو الباقي
        public string SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        //   public DateTime GeneratedDate { get; set; }

    }
}