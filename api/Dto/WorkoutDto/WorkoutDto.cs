using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace api.Dto.WorkoutDto
{
    public class WorkoutDto
    {
        public string? Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> ExerciseIds { get; set; } = new List<string>();

        public string UserId { get; set; } = string.Empty;

        public List<string> TagsIds { get; set; } = new List<string>();
    }
}
