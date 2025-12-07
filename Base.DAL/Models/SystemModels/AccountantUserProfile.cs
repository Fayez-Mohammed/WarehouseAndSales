using Base.DAL.Models.BaseModels;

public class AccountantUserProfile : BaseEntity
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
     
    }

