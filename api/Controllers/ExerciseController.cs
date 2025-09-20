using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interface;
using api.Mappers;
using api.Mappers.ExerciseMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/exercises")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {

        private readonly IExerciseRepository _exerRepo;
        public ExerciseController(IExerciseRepository exerRepo)
        {
            _exerRepo = exerRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var exercises = await _exerRepo.GetAllAsync();
            var exercisesDto = exercises.Select(e => e.ToExerciseDto()).ToList();
            return Ok(exercises);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            var exerciseModel = await _exerRepo.GetByIdAsync(id);
            if (exerciseModel == null)
            {
                return NotFound($"There is no exercise with id: {id}");
            }
            return Ok(exerciseModel.ToExerciseDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            var exerciseModel = await _exerRepo.DeleteAsync(id);

            if (exerciseModel == null)
            {
                return NotFound($"There is no exercise with id: {id}");
            }
            return NoContent();
        }
    }
}