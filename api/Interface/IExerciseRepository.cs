using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerciseDTOs;
using api.Helpers;
using api.Models;

namespace api.Interface
{
    public interface IExerciseRepository
    {
        Task<List<Exercise>> GetAllAsync(QueryObjectForExercises query);
        Task<Exercise?> GetByIdAsync(string id);
        Task<Exercise?> DeleteAsync(string id);
        Task<Exercise> CreateAsync(Exercise exerciseModel);
        Task<Exercise?> UpdateAsync(string Id, Exercise exerciseModel);
        Task<bool> ExistsByNameAsync(string name);
        
    }
}