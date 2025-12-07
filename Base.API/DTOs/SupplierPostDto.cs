using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs;

public class SupplierPostDto
{
    [Required]
    [MaxLength(250)]
    public string Name { get; set; }
    [Required]
    [MaxLength(250)]
    public string? Address { get; set; }



    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }

}