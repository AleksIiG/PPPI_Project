using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dto.ExerciseDTOs
{
    public class ExerciseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> TagsIds { get; set; } = new();
    }

    public class CreateExerciseDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string Description { get; set; } = string.Empty;

        public List<string> TagsIds { get; set; } = new();
    }

    public class UpdateExerciseDto
    {
        public string Id { get; set; } = string.Empty;
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string Name { get; set; } = string.Empty;
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string Description { get; set; } = string.Empty;
        public List<string> TagsIds { get; set; } = new();
    }
}