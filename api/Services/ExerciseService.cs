using api.Dto.ExerciseDTOs;
using api.Dto.ExerTagsDto;
using api.Interface;
using api.Mappers.ExerciseMapper;
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

        public async Task<List<ExerciseDto>> GetByIdsFromWorkoutsAsync(IEnumerable<string> exerciseIds)
        {
            var exerciseIdsList = exerciseIds.ToList();
            var exercises = new List<Exercise>();

            // Отримуємо всі вправи
            foreach (var id in exerciseIdsList)
            {
                var exercise = await _exerciseRepo.GetByIdAsync(id);
                if (exercise != null)
                {
                    exercises.Add(exercise);
                }
            }

            // Отримуємо всі теги для цих вправ
            var allTagIds = exercises
                .SelectMany(e => e.TagsIds)
                .Distinct()
                .ToList();

            var tags = await _exerTagService.GetByIdsFromExercisesAsync(allTagIds);
            var tagDict = tags.ToDictionary(t => t.Id);

            // Маппимо кожну вправу з її тегами
            return exercises.Select(e =>
            {
                var exerciseTags = e.TagsIds
                    .Where(id => tagDict.ContainsKey(id))
                    .Select(id => tagDict[id])
                    .ToList();

                return e.ToExerciseDto(exerciseTags);
            }).ToList();
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