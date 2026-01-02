namespace Base.API.DTOs
{
    internal class OrderItemDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string CustomerId { get; set; }
        public string ProductName { get; set; }
    }
}