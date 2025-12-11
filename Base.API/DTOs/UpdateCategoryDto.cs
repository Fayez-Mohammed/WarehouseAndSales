using System.ComponentModel.DataAnnotations;

namespace Base.API.DTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}