using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs;

public class ProductDto
{
    // public string? ProductId { get; set; }

    public string? ProductName { get; set; }

    [Required]
    [Range(0.1, int.MaxValue)]
    public decimal SalePrice { get; set; }
    [Required]
    [Range(0.1, int.MaxValue)]
    public decimal BuyPrice { get; set; }

    public int Quantity { get; set; }

    public string? SKU { get; set; }

    public string? Description { get; set; }
    public string? CategoryId { get; set; }

}

public class ProductWithCategoryNameDto
{
    // public string? ProductId { get; set; }

    public string? ProductName { get; set; }

    [Required]
    [Range(0.1, int.MaxValue)]
    public decimal SalePrice { get; set; }
    [Required]
    [Range(0.1, int.MaxValue)]
    public decimal BuyPrice { get; set; }

    public int Quantity { get; set; }

    public string? SKU { get; set; }

    public string? Description { get; set; }
    public string? CategoryName { get; set; }

}

public class ProductForUpdateDto
{
public string? ProductId { get; set; }
    public int Quantity { get; set; }
}