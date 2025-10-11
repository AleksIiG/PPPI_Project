using Amazon.Util;
using api.Dto.ExerTagsDto;
using api.Helpers;
using api.Interface;
using api.Mappers;
using api.Mappers.ExerTagMapper;
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
    [Route("api/exercisesTags")]
    [ApiController]
    public class ExerTagController: ControllerBase
    {
        private readonly IExerTagService _exerTagService;

        public ExerTagController(IExerTagService exerTagService)
        {
            _exerTagService = exerTagService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryObjectForTags query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var tags = await _exerTagService.GetAllAsync(query);
            if (tags == null)
            {
                return NotFound();
            }
            return Ok(tags.Select(t=>t.ToExerTagDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var tag = await _exerTagService.GetByIdAsync(id);
                return Ok(tag.ToExerTagDto());
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateExerTagDto exerTagDto)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var tag = exerTagDto.ToExerTagFromCreateDto();
            try
            {
                var createdTag = await _exerTagService.CreateAsync(tag);
                return CreatedAtAction(nameof(GetById), new { id = createdTag.Id }, tag);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateAsync([FromBody] UpdateExerTagDto exerTagDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = exerTagDto.ToExerTagFromUpdateDto();

            try
            {
                var updatedTag = await _exerTagService.UpdateAsync(tag, id);
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
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            try
            {
                await _exerTagService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
