using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interface;
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
            return Ok(exercises);
        }
    }
}