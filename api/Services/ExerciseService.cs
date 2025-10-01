using api.Dto.ExerTagsDto;
using api.Interface;
using api.Models;
using api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepo;
        private readonly IExerTagService _exerTagService;
        public ExerciseService(IExerciseRepository exerciseRepo, IExerTagService exerTagRepo)
        {
            _exerciseRepo = exerciseRepo;
            _exerTagService = exerTagRepo;
        }
        public async Task<Exercise> CreateAsync(Exercise exercise)
        {
            var nonExistingTags = await _exerTagService.GetNonExistingTagsAsync(exercise.TagsIds);
            var num = nonExistingTags.Count();
            if (num>0) 
            {
                throw new InvalidOperationException("The follow ID dose not exsist.");
            }

            if (await _exerciseRepo.ExistsByNameAsync(exercise.Name))
            {
                throw new InvalidOperationException($"Exercise '{exercise.Name}' already exists.");
            }
            
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

            var nonExistingTags = await _exerTagService.GetNonExistingTagsAsync(exercise.TagsIds);
            var num = nonExistingTags.Count();
            if (num > 0)
            {
                throw new InvalidOperationException("The follow ID dose not exsist.");
            }

            exercise.Id = id;
            await _exerciseRepo.UpdateAsync(id, exercise);
            return exercise;
        }
    }
}