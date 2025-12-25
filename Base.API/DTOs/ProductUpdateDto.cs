using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs;

public class ProductUpdateDto
{
    [Required]
    [Key]
    public string? ProductId { get; set; }
    [MaxLength(200)]
    public string? ProductName { get; set; }
    [Range(0.1, double.MaxValue)]
    public decimal SellPrice { get; set; }
    [Range(0, double.MaxValue)]
    public int Quantity { get; set; }
    [MaxLength(200)]
    public string? SKU { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    [Range(0.1, double.MaxValue)]
    public decimal BuyPrice { get; set; }
    [MaxLength(200)]
    public string? CategoryId { get; set; }
}
public class ProductUpdateWithCategoryNameDto
{
    [Required]
    [Key]
    public string? ProductId { get; set; }
    [MaxLength(200)]
    public string? ProductName { get; set; }
    [Range(0.1, double.MaxValue)]
    public decimal SellPrice { get; set; }
    [Range(0, double.MaxValue)]
    public int Quantity { get; set; }
    [MaxLength(200)]
    public string? SKU { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    [Range(0.1, double.MaxValue)]
    public decimal BuyPrice { get; set; }
    [MaxLength(200)]
    public string? CategoryName { get; set; }
}