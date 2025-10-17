using api.Helpers;
using api.Models;

namespace api.Interface
{
    public interface IWorkoutTagRepository
    {
        Task<List<WorkoutTag>> GetByIdsFromWorkoutsAsync(IEnumerable<string> tagIds);

        Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds);

        Task<List<WorkoutTag>> GetAllAsync(QueryObjectForTags query);
        Task<WorkoutTag?> GetByIdAsync(string id);

        Task<WorkoutTag> CreateAsync(WorkoutTag workoutTag);

        Task<WorkoutTag> UpdateAsync(WorkoutTag workoutTag, string id);

        Task<WorkoutTag> DeleteAsync(string id);
    }
}
