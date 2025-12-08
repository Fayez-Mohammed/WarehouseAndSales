using Base.DAL.Models.BaseModels;



namespace Base.DAL.Models.SystemModels
{
    public class Expense : BaseEntity
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Accountant User (who created this expense)
        public string? AccountantUserId { get; set; }
        public virtual ApplicationUser? AccountantUser { get; set; }
    }
}

