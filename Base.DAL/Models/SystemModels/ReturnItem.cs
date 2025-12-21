using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels; // Assuming ApplicationUser & BaseEntity are here

public class ReturnItem : BaseEntity
{
    public string ReturnRequestId { get; set; }
    public virtual ReturnRequest ReturnRequest { get; set; }

    public string ProductId { get; set; }
    public virtual Product Product { get; set; }

    public int Quantity { get; set; }
    public string? Reason { get; set; } // e.g. "Damaged", "Wrong Item"
}