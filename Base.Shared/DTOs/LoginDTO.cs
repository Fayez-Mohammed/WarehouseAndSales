using System.ComponentModel.DataAnnotations;

namespace Base.Shared.DTOs
{
    public class LoginDTO
    {
        //[Required]
        // [EmailAddress]
        public string? Email { get; set; } = "manager@test.com";

        [Required]
        [MinLength(4)]
        public string Password { get; set; }
    }
}
