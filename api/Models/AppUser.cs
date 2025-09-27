using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class AppUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [BsonElement("passwordHash")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string PasswordHash { get; set; } = string.Empty;


        [BsonElement("likedWorkoutsIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> LikedWorkouts { get; set; } = new();

        [BsonElement("createdWorkoutsIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> CreatedWorkouts { get; set; } = new();

        [BsonElement("weight")]
        public double Weight { get; set; }

        [BsonElement("height")]
        public double Height { get; set; }

        [BsonElement("age")]
        public int Age { get; set; }
        
        [BsonElement("role")]
        public string Role { get; set; } = "User"; // "User", "Admin"


        

    }
}