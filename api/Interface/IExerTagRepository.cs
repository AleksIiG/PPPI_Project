using System.Threading.Tasks;
using api.Dto;
using api.Models;

namespace api.Interface
{
    public interface IExerTagRepository
    {
        Task<List<ExerciseTag>> GetByIdsFromExercisesAsync(IEnumerable<string> tagIds);

        Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds);
    }
}
