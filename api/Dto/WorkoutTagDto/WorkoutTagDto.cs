using System.ComponentModel.DataAnnotations;

namespace api.Dto.WorkoutTagDto
{
    public class WorkoutTagDto
    {
        public string? Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
