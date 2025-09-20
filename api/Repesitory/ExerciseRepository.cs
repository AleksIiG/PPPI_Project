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

        public ExerciseRepository(MongoDbService database)
        {
            _database = database;
        }



        public async Task<List<Exercise>> GetAllAsync()
        {
            return await _database.Exercises.Find(_ => true).ToListAsync();
        }

        public async Task<Exercise?> GetByIdAsync(string id)
        {
            return await _database.Exercises.Find(e => e.Id == id).FirstOrDefaultAsync();
        }
        public async Task<Exercise?> DeleteAsync(string id)
        {

            var exerciseModel = await _database.Exercises.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (exerciseModel == null)
            {
                return null;
            }

            await _database.Exercises.DeleteOneAsync(e => e.Id == id);
            return exerciseModel;
        }

        public Task<Exercise?> Create(Exercise commentModel)
        {
            throw new NotImplementedException();
        }
    }
}