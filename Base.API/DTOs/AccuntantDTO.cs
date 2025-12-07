// Base.API.DTOs/AccountantPostDto.cs
using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs;

public class AccountantPostDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string PhoneNumber { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
}