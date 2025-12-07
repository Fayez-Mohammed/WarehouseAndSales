using Base.DAL.Models.BaseModels; // Assuming ApplicationUser & BaseEntity are here

namespace Base.DAL.Models.SystemModels
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; }
      
        public string? Address { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual List<Product> Products { get; set; }
        public virtual ICollection<StockTransaction> SupplyTransactions { get; set; }
    }
}