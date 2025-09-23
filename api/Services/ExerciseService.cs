using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interface;
using api.Models;
using api.Services.Interfaces;

namespace api.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepo;
        public ExerciseService(IExerciseRepository exerciseRepo)
        {
            _exerciseRepo = exerciseRepo;
        }
        public async Task<Exercise> CreateAsync(Exercise exercise)
        {
            if (await _exerciseRepo.ExistsByNameAsync(exercise.Name))
            {
                throw new InvalidOperationException($"Exercise '{exercise.Name}' already exists.");
            }
            // TODO: перевірка тегі
            return await _exerciseRepo.CreateAsync(exercise);
        }
        



        public async Task<Exercise?> DeleteAsync(string id)
        {
            var existing = await _exerciseRepo.GetByIdAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Exercise with id {id} not found.");
            }

            return await _exerciseRepo.DeleteAsync(id);

        }




        public async Task<List<Exercise>> GetAllAsync()
        {
            return await _exerciseRepo.GetAllAsync();
        }

        public async Task<Exercise> GetByIdAsync(string id)
        {
            var exercise = await _exerciseRepo.GetByIdAsync(id);

            if (exercise == null)
            {
                throw new KeyNotFoundException($"Exercise with id {id} not found.");
            }
            return exercise;
            
        }



        

        public async Task<Exercise> UpdateAsync(string id, Exercise exercise)
        {
            var existingExercise = await _exerciseRepo.GetByIdAsync(id);
            if (existingExercise == null)
            {
                throw new KeyNotFoundException($"Exercise with id {id} not found.");
            }
            if (existingExercise.Name != exercise.Name && await _exerciseRepo.ExistsByNameAsync(exercise.Name))
                throw new InvalidOperationException($"Exercise with name '{exercise.Name}' already exists.");

            exercise.Id = id;
            await _exerciseRepo.UpdateAsync(id, exercise);
            return exercise;
        }
    }
}