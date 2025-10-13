using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using MongoDB.Driver;


namespace api.Data
{
    public class MongoDbService
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        public MongoDbService(IConfiguration configuration)
        {
            _client = new MongoClient(configuration["MONGO_CONNECTION_STRING"]);
            _database = _client.GetDatabase(configuration["MONGO_DATABASE_NAME"]);
        }

        public IMongoCollection<AppUser> Users => _database.GetCollection<AppUser>("Users");
        public IMongoCollection<Workout> Workouts => _database.GetCollection<Workout>("Workouts");
        public IMongoCollection<Exercise> Exercises => _database.GetCollection<Exercise>("Exercises");
        public IMongoCollection<ExerciseTag> ExerciseTags => _database.GetCollection<ExerciseTag>("ExerciseTags");
        public IMongoCollection<WorkoutTag> WorkoutTags => _database.GetCollection<WorkoutTag>("WorkoutTags");
        public IMongoCollection<RevorkedToken> RevorkedTokens => _database.GetCollection<RevorkedToken>("RevorkedTokens");
    }
}


// Замінити значення в appsetings