using api.Data;
using api.Dto.WorkoutDto;
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
    public class WorkoutRepository: IWorkoutRepository
    {
        private readonly MongoDbService _database;

        public WorkoutRepository(MongoDbService database)
        {
            _database = database;
        }

        public async Task<List<Workout>> GetAllAsync()
        {
            return await _database.Workouts.Find(_ => true).ToListAsync();
        }

        public async Task<Workout?> GetByIdAsync(string id)
        {
            return await _database.Workouts.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Workout> CreateAsync(Workout workoutModel)
        {
            await _database.Workouts.InsertOneAsync(workoutModel);
            return workoutModel;
        }

        public async Task<Workout?> UpdateAsync(string id, Workout workoutModel)
        {
            var updated = await _database.Workouts.ReplaceOneAsync(w => w.Id == id, workoutModel);
            if (updated == null)
            {
                return null;
            }
            return workoutModel;
        }

        public async Task<Workout?> DeleteAsync(string id)
        {
            var model = await _database.Workouts.Find(w => w.Id == id).FirstOrDefaultAsync();
            if (model == null)
            {
                return null;
            }
            await _database.Workouts.DeleteOneAsync(w => w.Id == id);
            return model;

        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _database.Workouts.Find(w=>w.Name==name).AnyAsync();
        }
    }
}
