using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    [BsonIgnoreExtraElements]
    public class AppUser
    {
        [BsonId] // Позначає, що це ключ
        [BsonRepresentation(BsonType.ObjectId)] // Дозволяє працювати з рядком замість ObjectId
        public string? Id { get; set; }  // Поле для _id в MongoDB

        [BsonElement("name")] // Назва поля в БД
        public string Username { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;
        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;
    }
}