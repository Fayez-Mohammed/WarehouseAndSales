using Base.DAL.Models.BaseModels; // Assuming ApplicationUser & BaseEntity are here




namespace Base.DAL.Models.SystemModels
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }

        // Navigation Property
        public virtual ICollection<Product> Products { get; set; }
    }
}