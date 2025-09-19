using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace api.Models
{
    public class Workout
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = string.Empty;
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

        // TODO: Image
        // TODO: Shablon Tags
        

        // TODO: Change Exercises to list of Exercises IDs
    }
}