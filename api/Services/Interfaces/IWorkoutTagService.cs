using api.Dto.ExerTagsDto;
using api.Dto.WorkoutTagDto;
using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services.Interfaces
{
    public interface IWorkoutTagService
    {
        Task<List<WorkoutTagDto>> GetByIdsFromWorkoutsAsync(IEnumerable<string> tagIds);

        Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds);

        Task<List<WorkoutTag>> GetAllAsync();

        Task<WorkoutTag> GetByIdAsync(string id);

        Task<WorkoutTag> CreateAsync(WorkoutTag exerciseTag);

        Task<WorkoutTag> UpdateAsync(WorkoutTag exerciseTag, string id);

        Task<WorkoutTag> DeleteAsync(string id);
    }
}
