using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using api.Dto.ExerciseDTOs;
using api.Helpers;

using api.Interface;
using api.Mappers;
using api.Mappers.ExerciseMapper;
using api.Services.Interfaces;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/exercises")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {


        private readonly IExerciseService _exerciseService;
        private readonly IExerTagService _exerTagService;

        public ExerciseController(IExerciseService exerciseService, IExerTagService exerTagService)
        {
            _exerciseService = exerciseService;
            _exerTagService = exerTagService;

        }


        [HttpGet]

        public async Task<IActionResult> GetAll([FromQuery] QueryObjectForExercises query)
        {
            var exercises = await _exerciseService.GetAllAsync(query);

            // ✅ Отримуємо всі унікальні ID тегів
            var allTagIds = exercises
                .SelectMany(e => e.TagsIds)
                .Distinct()
                .ToList();

            // ✅ Викликаємо сервіс тегів
            var tags = await _exerTagService.GetByIdsFromExercisesAsync(allTagIds);
            var tagDict = tags.ToDictionary(t => t.Id);

            if (!string.IsNullOrEmpty(query.TagName))
            {
                var matchedTagIds = tags.Where(t => string.Equals(t.Name, query.TagName, StringComparison.OrdinalIgnoreCase))
                    .Select(t => t.Id)
                    .ToHashSet();

                exercises = exercises
                    .Where(e => e.TagsIds.Any(id => matchedTagIds.Contains(id)))
                    .ToList();
            }

            // ✅ Маппимо кожну вправу з її тегами
            var exercisesDto = exercises.Select(e =>
            {
                var exerciseTags = e.TagsIds
                    .Where(id => tagDict.ContainsKey(id))
                    .Select(id => tagDict[id])
                    .ToList();

                return e.ToExerciseDto(exerciseTags);
            }).ToList();


            return Ok(exercisesDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            try
            {

                var exercise = await _exerciseService.GetByIdAsync(id);

                // ✅ Отримуємо тільки потрібні теги
                var tags = await _exerTagService.GetByIdsFromExercisesAsync(exercise.TagsIds);

                return Ok(exercise.ToExerciseDto(tags));

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
                await _exerciseService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }


        }





        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExerciseDto exerciseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exercise = exerciseDto.ToExerciseFromCreateExerciseDto();


            try
            {
                var CREATEDexercise = await _exerciseService.CreateAsync(exercise);
                return CreatedAtAction(nameof(GetById), new { id = CREATEDexercise.Id }, CREATEDexercise);
            }

            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }



        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] UpdateExerciseDto exerciseDto, string id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exercise = exerciseDto.ToExerciseFromUpdateExerciseDto();
            try
            {
                var updated = await _exerciseService.UpdateAsync(id, exercise);

                if (updated == null)
                    throw new KeyNotFoundException($"Exercise with id {id} not found.");

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
    }
}