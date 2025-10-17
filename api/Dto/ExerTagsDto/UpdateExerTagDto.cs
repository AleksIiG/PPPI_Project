using System.ComponentModel.DataAnnotations;

namespace api.Dto.ExerTagsDto
{
    public class UpdateExerTagDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "TemplateTag name must be between 3 and 50 characters.")]
        public string Name { get; set; } = string.Empty;

    }
}
