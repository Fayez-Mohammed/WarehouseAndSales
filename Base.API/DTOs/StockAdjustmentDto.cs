// Base.API.DTOs/AccountantPostDto.cs
using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs;

public class StockAdjustmentDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public string ProductId { get; set; }

    [Required(ErrorMessage = "Actual Quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Actual Quantity must be 0 or greater")]
    public int ActualQuantity { get; set; }

}