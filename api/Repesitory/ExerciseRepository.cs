using api.Data;
using api.Dto.ExerciseDTOs;
using api.Helpers;
using api.Interface;
using api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Repesitory
{
    public class ExerciseRepository : IExerciseRepository
    {

        private readonly MongoDbService _database;

        public ExerciseRepository(MongoDbService database)
        {
            _database = database;
        }



        public async Task<List<Exercise>> GetAllAsync(QueryObjectForExercises query)
        {
            var filter = Builders<Exercise>.Filter.Empty;
            if (!string.IsNullOrEmpty(query.Name))
            {
                filter &= Builders<Exercise>.Filter.Eq(e => e.Name, query.Name);
            }
            return await _database.Exercises.Find(filter).ToListAsync();
        }        

        public async Task<Exercise?> GetByIdAsync(string id)
        {
            return await _database.Exercises.Find(e => e.Id == id).FirstOrDefaultAsync();
        }
        public async Task<Exercise?> DeleteAsync(string id)
        {

            var exerciseModel = await _database.Exercises.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (exerciseModel == null) return null;
            await _database.Exercises.DeleteOneAsync(e => e.Id == id);
            return exerciseModel;
        }


        public async Task<Exercise> CreateAsync(Exercise exerciseModel)
        {
            await _database.Exercises.InsertOneAsync(exerciseModel);
            return exerciseModel;
        }

        public async Task<Exercise?> UpdateAsync(string Id, Exercise exerciseModel)
        {
            var result = await _database.Exercises.ReplaceOneAsync(e => e.Id == Id, exerciseModel);
            if (result.MatchedCount == 0) return null;
            return exerciseModel;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _database.Exercises.Find(e => e.Name == name).AnyAsync();
        }

    }
}