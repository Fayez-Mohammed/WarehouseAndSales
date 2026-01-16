public class ReturnToSupplierRequestDto
{
    public required string SupplierName { get; set; }
    public List<ReturnItemForSupplierDto> Items { get; set; }
}
public class ReturnItemForSupplierDto
{
    public required string ProductName { get; set; } // أو ProductId
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}