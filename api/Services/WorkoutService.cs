using api.Dto.ExerTagsDto;
using api.Helpers;
using api.Interface;
using api.Models;
using api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepository _workoutRepo;
        private readonly IExerciseService _exerciseService;
        private readonly IWorkoutTagService _workoutTagService;

        public WorkoutService(IWorkoutRepository workoutRepository, IExerciseService exerciseService, IWorkoutTagService workoutTagService)
        {
            _workoutRepo = workoutRepository;
            _exerciseService = exerciseService;
            _workoutTagService = workoutTagService;
        }
        public async Task<Workout> CreateAsync(Workout workout)
        {
            // Перевіряємо теги тренування
            var nonExistingTags = await _workoutTagService.GetNonExistingTagsAsync(workout.TagsIds);
            if (nonExistingTags.Count > 0)
            {
                throw new InvalidOperationException("Some tag IDs do not exist.");
            }

            // Перевіряємо вправи
            foreach (var exerciseId in workout.ExerciseIds)
            {
                try
                {
                    await _exerciseService.GetByIdAsync(exerciseId);
                }
                catch (KeyNotFoundException)
                {
                    throw new InvalidOperationException($"Exercise with id {exerciseId} does not exist.");
                }
            }

            // Перевіряємо унікальність імені
            if (await _workoutRepo.ExistsByNameAsync(workout.Name))
            {
                throw new InvalidOperationException($"Workout '{workout.Name}' already exists.");
            }

            return await _workoutRepo.CreateAsync(workout);
        }

        public async Task<Workout?> DeleteAsync(string id)
        {
            var existing = await _workoutRepo.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Workout with id {id} not found.");
            }

            return await _workoutRepo.DeleteAsync(id);
        }

        public async Task<List<Workout>> GetAllAsync(QueryObjectForWorkouts query)
        {
            return await _workoutRepo.GetAllAsync(query);
        }

        public async Task<Workout> GetByIdAsync(string id)
        {
            var result = await _workoutRepo.GetByIdAsync(id);
            if (result == null)
            {
                throw new KeyNotFoundException($"Workout with id {id} not found.");
            }
            return result;
        }

        public async Task<Workout> UpdateAsync(string id, Workout workout)
        {
            var existingWorkout = await _workoutRepo.GetByIdAsync(id);
            if (existingWorkout == null)
            {
                throw new KeyNotFoundException($"Workout with id {id} not found.");
            }

            // Перевіряємо ім'я
            if (existingWorkout.Name != workout.Name && await _workoutRepo.ExistsByNameAsync(workout.Name))
            {
                throw new InvalidOperationException($"Workout with name '{workout.Name}' already exists.");
            }

            // Перевіряємо теги
            var nonExistingTags = await _workoutTagService.GetNonExistingTagsAsync(workout.TagsIds);
            if (nonExistingTags.Count > 0)
            {
                throw new InvalidOperationException("Some tag IDs do not exist.");
            }

            // Перевіряємо вправи
            foreach (var exerciseId in workout.ExerciseIds)
            {
                try
                {
                    await _exerciseService.GetByIdAsync(exerciseId);
                }
                catch (KeyNotFoundException)
                {
                    throw new InvalidOperationException($"Exercise with id {exerciseId} does not exist.");
                }
            }

            workout.Id = id;
            await _workoutRepo.UpdateAsync(id, workout);
            return workout;
        }
    }
}
