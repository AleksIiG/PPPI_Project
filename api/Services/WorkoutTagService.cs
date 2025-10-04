using api.Dto;
using api.Dto.ExerTagsDto;
using api.Dto.WorkoutTagDtos;
using api.Interface;
using api.Models;
using api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services
{
    public class WorkoutTagService: IWorkoutTagService
    {
        private readonly IWorkoutTagRepository _workoutTagRepo;

        public WorkoutTagService(IWorkoutTagRepository workoutTagRepo) 
        {
            _workoutTagRepo = workoutTagRepo;
        }

        public async Task<List<WorkoutTagDto>> GetByIdsFromWorkoutsAsync(IEnumerable<string> workIds)
        {
            var tags = await _workoutTagRepo.GetByIdsFromWorkoutsAsync(workIds);

            return tags.Select(tag => new WorkoutTagDto
            {
                Id = tag.Id,
                Name = tag.Name
            }).ToList();
        }

        public async Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds)
        {
            var tagIdsList = tagIds.ToList();
            var existingTagIds = await _workoutTagRepo.GetNonExistingTagsAsync(tagIdsList);
            return tagIdsList.Except(existingTagIds).ToList();
        }

        public async Task<List<WorkoutTag>> GetAllAsync()
        {
            return await _workoutTagRepo.GetAllAsync();
        }

        public async Task<WorkoutTag> GetByIdAsync(string id)
        {
            return await _workoutTagRepo.GetByIdAsync(id);
        }

        public async Task<WorkoutTag> CreateAsync(WorkoutTag workoutTag)
        {
            return await _workoutTagRepo.CreateAsync(workoutTag);
        }

        public async Task<WorkoutTag> UpdateAsync(WorkoutTag workoutTag, string id)
        {
            workoutTag.Id = id;
            await _workoutTagRepo.UpdateAsync(workoutTag, id);
            return workoutTag;
        }

        public async Task<WorkoutTag> DeleteAsync(string id)
        {
            var exist = await _workoutTagRepo.GetByIdAsync(id);
            if (exist == null)
            {
                throw new KeyNotFoundException($"Tag with id: {id} is not found.");
            }

            return await _workoutTagRepo.DeleteAsync(id);
        }

    }
}
