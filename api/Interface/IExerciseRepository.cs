using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerciseDTOs;
using api.Models;

namespace api.Interface
{
    public interface IExerciseRepository
    {
        Task<List<Exercise>> GetAllAsync();
        Task<List<ExerciseTag>> GetAllExerciseTagsAsync();
        Task<Exercise?> GetByIdAsync(string id);
        Task<Exercise?> DeleteAsync(string id);
        Task<Exercise> CreateAsync(Exercise exerciseModel);
        Task<Exercise?> UpdateAsync(string Id, Exercise exerciseModel);
        Task<bool> ExistsByNameAsync(string name);
        Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds);
    }
}