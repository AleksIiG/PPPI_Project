using api.Dto;
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
    public class ExerTagService: IExerTagService
    {
        private readonly IExerTagRepository _exerTagRepo;

        public ExerTagService(IExerTagRepository exerTagRepo)
        {
            _exerTagRepo = exerTagRepo;
        }

        public async Task<List<ExerTagDto>> GetByIdsFromExercisesAsync(IEnumerable<string> tagIds)
        {
            var tags = await _exerTagRepo.GetByIdsFromExercisesAsync(tagIds);

            return tags.Select(tag => new ExerTagDto
            {
                Id = tag.Id,
                Name = tag.Name
            }).ToList();
        }

        public async Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds)
        {
            var tagIdsList = tagIds.ToList();
            var existingTagIds = await _exerTagRepo.GetNonExistingTagsAsync(tagIdsList);
            return tagIdsList.Except(existingTagIds).ToList();
        }

        public async Task<List<ExerciseTag>> GetAllAsync()
        {
            return await _exerTagRepo.GetAllAsync();
        }

        public async Task<ExerciseTag> GetByIdAsync(string id)
        {
            return await _exerTagRepo.GetByIdAsync(id);
        }

        public async Task<ExerciseTag> CreateAsync(ExerciseTag exerciseTag)
        {
            return await _exerTagRepo.CreateAsync(exerciseTag);
        }

        public async Task<ExerciseTag> UpdateAsync(ExerciseTag exerciseTag, string id)
        {
            exerciseTag.Id = id;
            await _exerTagRepo.UpdateAsync(exerciseTag, id);
            return exerciseTag;
        }

        public async Task<ExerciseTag> DeleteAsync(string id) 
        {
            var exist = await _exerTagRepo.GetByIdAsync(id);
            if (exist == null) 
            {
                throw new KeyNotFoundException($"Tag with id: {id} is not found.");
            }

            return await _exerTagRepo.DeleteAsync(id);
        }
    }
}
