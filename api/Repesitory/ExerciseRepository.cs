using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dto.ExerciseDTOs;
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

        public async Task<Exercise?> CreateAsync(Exercise exerciseModel)
        {
            var existExercise = await _database.Exercises.Find(e => e.Name == exerciseModel.Name).AnyAsync();
            if (existExercise)
            {
                throw new InvalidOperationException($"Exercise '{exerciseModel.Name}' already exists.");
            }

            //Unique index for not double routes in db
            var indexKeys = Builders<Exercise>.IndexKeys.Ascending(e => e.Name);
            await _database.Exercises.Indexes.CreateOneAsync(
                new CreateIndexModel<Exercise>(indexKeys, new CreateIndexOptions { Unique = true })
            );

            await _database.Exercises.InsertOneAsync(exerciseModel);
            return exerciseModel;

            //TODO: Придумати як зробити перевірку на Тегах
            //TODO: Придумати як зробити перевірку на Тегах
            //TODO: Придумати як зробити перевірку на Тегах
            //TODO: Придумати як зробити перевірку на Тегах

        }

        public async Task<Exercise?> UpdateAsync(string Id, Exercise exerciseModel)
        {
            var UpdatedExercise = await _database.Exercises.Find(e => e.Id == Id).FirstOrDefaultAsync();
            if (UpdatedExercise == null)
            {
                throw new InvalidOperationException($"Exercise with id {Id} not found.");
            }
            if (UpdatedExercise.Name != exerciseModel.Name && await ExistsByNameAsync(exerciseModel.Name))
                throw new InvalidOperationException($"Exercise with name '{exerciseModel.Name}' already exists.");

            exerciseModel.Id = Id;
            await _database.Exercises.ReplaceOneAsync(e => e.Id == Id, exerciseModel);
            return exerciseModel;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _database.Exercises.Find(e => e.Name == name).AnyAsync();
        }

        
    }
}