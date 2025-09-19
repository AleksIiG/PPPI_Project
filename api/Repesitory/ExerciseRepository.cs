using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interface;
using api.Models;
using MongoDB.Driver;

namespace api.Repesitory
{
    public class ExerciseRepository : IExerciseRepository
    {

        private readonly MongoDbService _database;

        public ExerciseRepository( MongoDbService database)
        {
            _database = database;
        }


        public async Task<List<Exercise>> GetAllAsync()
        {
            return await _database.Exercises.Find(_ => true).ToListAsync();
        }

        public Task<Exercise?> GetById(string id)
        {
            throw new NotImplementedException();
        }
    }
}