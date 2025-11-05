using api.Dto.WorkoutTagDtos;
using api.Helpers;
using api.Mappers.WorkoutMapper;
using api.Services;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/workoutsTags")]
    [ApiController]

    public class WorkoutTagController : ControllerBase
    {
        private readonly IWorkoutTagService _workoutTagService;

        public WorkoutTagController(IWorkoutTagService workoutTagService)
        {
            _workoutTagService = workoutTagService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryObjectForTags query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var tags = await _workoutTagService.GetAllAsync(query);
            if (tags == null)
            {
                return NotFound();
            }
            return Ok(tags.Select(t => t.ToWorkoutTagDto()));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var tag = await _workoutTagService.GetByIdAsync(id);
                return Ok(tag.ToWorkoutTagDto());
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateWorkoutTagDto workTagDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = workTagDto.ToWorkoutTagFromCreateDto();
            try
            {
                var createdTag = await _workoutTagService.CreateAsync(tag);
                return CreatedAtAction(nameof(GetById), new { id = createdTag.Id }, tag);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateWorkoutTagDto workTagDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = workTagDto.ToWorkoutTagFromUpdateDto();

            try
            {
                var updatedTag = await _workoutTagService.UpdateAsync(tag, id);
                if (updatedTag == null)
                {
                    throw new KeyNotFoundException($"Tag with Id: {id} is not found.");
                }
                return Ok(updatedTag);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            try
            {
                await _workoutTagService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
