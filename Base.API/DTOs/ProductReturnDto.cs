using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs;

public class ProductReturnDto
{
    [Required]
    [MaxLength(200)]
    public string? ProductId { get; set; }
    [MaxLength(200)]
    public string? ProductName { get; set; }
    [Required]
    [Range(0.1, int.MaxValue)]
    public decimal SalePrice { get; set; }
    [Required]
    [Range(0.1, int.MaxValue)]
    public decimal BuyPrice { get; set; }
    public int Quantity { get; set; }
    [MaxLength(200)]
    public string? SKU  { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    [MaxLength(200)]
    public string? CategoryName { get; set; } = null;
    public string? CategoryId { get; set; }
}