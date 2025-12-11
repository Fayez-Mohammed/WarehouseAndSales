using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs
{
    // =========================================================
    // DTOs (You can move these to Base.Shared.DTOs)
    // =========================================================

    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }
    }

 
}