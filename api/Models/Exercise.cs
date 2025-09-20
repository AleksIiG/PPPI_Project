using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class Exercise
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Exercise name must be between 3 and 50 characters.")]
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("tagsIds")]
        public List<string> TagsIds { get; set; } = new List<string>();
        
        
    }
}