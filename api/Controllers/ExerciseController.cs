using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerciseDTOs;
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

        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var exercises = await _exerciseService.GetAllAsync();
            var exercisesDto = exercises.Select(e => e.ToExerciseDto()).ToList();
            return Ok(exercisesDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            try
            {
                var exerciseModel = await _exerciseService.GetByIdAsync(id);
                return Ok(exerciseModel.ToExerciseDto());
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
                var createdExercise = await _exerciseService.CreateAsync(exercise);
                return CreatedAtAction(nameof(GetById), new { id = createdExercise.Id }, createdExercise);
            }

            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

            //TODO: Придумати як зробити перевірку на Тегах
            //TODO: Придумати як зробити перевірку на Тегах
            //TODO: Придумати як зробити перевірку на Тегах
            //TODO: Придумати як зробити перевірку на Тегах

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