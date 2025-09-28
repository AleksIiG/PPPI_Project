using api.Data;
using api.Dto.ExerciseDTOs;
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



        public async Task<List<Exercise>> GetAllAsync()
        {
            return await _database.Exercises.Find(_ => true).ToListAsync();
        }
        public async Task<List<ExerciseTag>> GetAllExerciseTagsAsync()
        {
            return await _database.ExerciseTags.Find(_ => true).ToListAsync();
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

        public async Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds)
        {
            var tagIdsList = tagIds.ToList();
            var objectIds = tagIdsList.Select(id => ObjectId.Parse(id)).ToList();

            var filter = Builders<ExerciseTag>.Filter.In("_id", objectIds);
            var existingTagIds = await _database.ExerciseTags
                .Find(filter)
                .Project(t => t.Id.ToString())
                .ToListAsync();

            return tagIdsList.Except(existingTagIds).ToList();
        }
    }
}