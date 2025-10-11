using System.Threading.Tasks;
using api.Dto;
using api.Helpers;
using api.Models;

namespace api.Interface
{
    public interface IExerTagRepository
    {
        Task<List<ExerciseTag>> GetByIdsFromExercisesAsync(IEnumerable<string> tagIds);
        Task<List<ExerciseTag>> GetByIdsFromExercisesAsync(IEnumerable<string> tagIds, QueryObjectForExercises query);

        Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds);

        Task<List<ExerciseTag>> GetAllAsync(QueryObjectForTags query);
        Task<ExerciseTag?> GetByIdAsync(string id);

        Task<ExerciseTag> CreateAsync(ExerciseTag exerciseTag);

        Task<ExerciseTag> UpdateAsync(ExerciseTag exerciseTag, string id);

        Task<ExerciseTag> DeleteAsync(string id);
    }
}
