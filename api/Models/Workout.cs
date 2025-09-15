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
        [BsonElement("exercises")]
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();

        // TODO: Image
        // TODO: Shablon Tags
        // TODO: Warm-up exercises
    }
}