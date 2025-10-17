using api.Dto.WorkoutDto;
using api.Helpers;
using api.Interface;
using api.Mappers;
using api.Mappers.WorkoutMapper;
using api.Models;
using api.Services;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/workouts")]
    [ApiController]
    public class WorkoutController: ControllerBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly IWorkoutTagService _workoutTagService;
        private readonly IExerciseService _exerciseService;

        public WorkoutController(IWorkoutService workoutService, IWorkoutTagService workoutTagService, IExerciseService exerciseService)
        {
            _workoutService = workoutService;
            _workoutTagService = workoutTagService;
            _exerciseService = exerciseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryObjectForWorkouts query)
        {
            var workouts = await _workoutService.GetAllAsync(query);

            // Отримуємо всі унікальні ID тегів тренувань
            var allTagIds = workouts
                .SelectMany(w => w.TagsIds)
                .Distinct()
                .ToList();

            // Отримуємо всі унікальні ID вправ
            var allExerciseIds = workouts
                .SelectMany(w => w.ExerciseIds)
                .Distinct()
                .ToList();

            // Викликаємо сервіси (як в ExerciseController)
            var workoutTags = await _workoutTagService.GetByIdsFromWorkoutsAsync(allTagIds);
            var workoutTagDict = workoutTags.ToDictionary(t => t.Id);

            if (!string.IsNullOrEmpty(query.TagName))
            {
                var matchedIds = workoutTags
                    .Where(t => string.Equals(t.Name, query.TagName, StringComparison.OrdinalIgnoreCase))
                    .Select(t => t.Id)
                    .ToHashSet();
                workouts = workouts.Where(w => w.TagsIds.Any(id => matchedIds.Contains(id))).ToList();
            }

            var exercises = await _exerciseService.GetByIdsFromWorkoutsAsync(allExerciseIds);
            var exerciseDict = exercises.ToDictionary(e => e.Id);

            // Маппимо кожен workout
            var workoutsDto = workouts.Select(w =>
            {
                var tags = w.TagsIds
                    .Where(id => workoutTagDict.ContainsKey(id))
                    .Select(id => workoutTagDict[id])
                    .ToList();

                var workoutExercises = w.ExerciseIds
                    .Where(id => exerciseDict.ContainsKey(id))
                    .Select(id => exerciseDict[id])
                    .ToList();

                return WorkoutMapper.ToWorkoutDto(w, workoutExercises, tags);
            }).ToList();

            return Ok(workoutsDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var workout = await _workoutService.GetByIdAsync(id);

                // Отримуємо теги та вправи
                var workoutTags = await _workoutTagService.GetByIdsFromWorkoutsAsync(workout.TagsIds);
                var exercises = await _exerciseService.GetByIdsFromWorkoutsAsync(workout.ExerciseIds);

                return Ok(WorkoutMapper.ToWorkoutDto(workout, exercises, workoutTags));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkoutDto workoutDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var workout = workoutDto.ToWorkoutFromCreateDto();

            try
            {
                var createdWorkout = await _workoutService.CreateAsync(workout);
                return CreatedAtAction(nameof(GetById), new { id = createdWorkout.Id }, createdWorkout);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] UpdateWorkoutDto workoutDto, string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var workout = workoutDto.ToWorkoutFromUpdateDto();

            try
            {
                var updated = await _workoutService.UpdateAsync(id, workout);
                if (updated == null)
                    throw new KeyNotFoundException($"Workout with id {id} not found.");
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                await _workoutService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
