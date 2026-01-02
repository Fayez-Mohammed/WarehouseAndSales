using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs
{
    public class CreateOrderDto
    {
        // Optional: The Sales Rep ID can be provided if a rep places the order for a customer.
        // If null, it can be assigned later during the confirmation process.
        public string? SalesRepId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
        public List<CreateOrderItemDto> Items { get; set; }
    }
    public class CreateOrderByManagerDto
    {
        // Optional: The Sales Rep ID can be provided if a rep places the order for a customer.
        // If null, it can be assigned later during the confirmation process.
        public string? SalesRepName { get; set; }
        [Required]
        
        public required string CustomerName { get; set; }
      //  public decimal? CommissionPercentage { get; set; } = 10m;

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
        public List<CreateOrderItemDto> Items { get; set; }
    }

    public class CreateOrderItemDto
    {
        [Required]
        public string ProductName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
