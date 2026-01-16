using System.ComponentModel.DataAnnotations;
namespace Base.API.DTOs
{
    public class CreateReturnRequestDto
    {
     //   [Required]
      //  public string CustomerId { get; set; }
        [Required]
        public int OrderCode { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Must return at least one item.")]
        public List<ReturnItemDto> Items { get; set; }
    }

    public class ReturnItemDto
    {
        [Required]
        public required string productName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public string? Reason { get; set; }
    }
}