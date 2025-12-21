using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels; // Assuming ApplicationUser & BaseEntity are here

public class ReturnRequest : BaseEntity
    {
    public enum ReturnStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public string OrderId { get; set; }
        public virtual Order Order { get; set; }

        public string CustomerId { get; set; }
        public virtual ApplicationUser Customer { get; set; }

        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
        public string? AdminComment { get; set; } // سبب الرفض أو ملاحظات الموافقة

        public virtual ICollection<ReturnItem> ReturnItems { get; set; }
    }
