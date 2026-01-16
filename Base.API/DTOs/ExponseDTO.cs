// Base.API.DTOs/CreateExpenseDto.cs
using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs;

public class CreateExpenseDto
{
    [Required]
    public decimal Amount { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    // Optional: Admin or Manager can specify who made the expense
   // public string? AccountantUserId { get; set; }
}


public class ExpenseResponseDto
{
    public string Id { get; set; }
    public int Code { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
  //  public string? AccountantUserId { get; set; }
    public string? AccountantName { get; set; }
}