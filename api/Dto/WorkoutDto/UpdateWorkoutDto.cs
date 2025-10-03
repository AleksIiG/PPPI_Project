using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace api.Dto.WorkoutDto
{
    public class UpdateWorkoutDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Workout name must be between 3 and 50 characters.")]
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [BsonElement("exerciseIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> ExerciseIds { get; set; } = new List<string>();
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("tagsIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> TagsIds { get; set; } = new List<string>();
    }
}
