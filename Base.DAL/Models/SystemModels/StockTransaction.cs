using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels.Enums;
using System.ComponentModel.DataAnnotations.Schema; // Assuming ApplicationUser & BaseEntity are here

namespace Base.DAL.Models.SystemModels
{
    public class StockTransaction : BaseEntity
    {
        public TransactionType Type { get; set; }
        public int Quantity { get; set; } // + for In, - for Out
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitBuyPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitSellPrice { get; set; }

        public string? Notes { get; set; } // Store "Profit: Found 5 items" here
        public string ProductId { get; set; }
        public virtual Product Product { get; set; }

        // The Store Manager who approved this
        public string? StoreManagerId { get; set; }
        public virtual ApplicationUser StoreManager { get; set; }

        // If it's Stock In, we might want to know which Supplier
        public string? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        // If it's Stock Out, link to the Order
        public string? OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}