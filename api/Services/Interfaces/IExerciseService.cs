using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerTagsDto;
using api.Models;

namespace api.Services.Interfaces
{
    public interface IExerciseService
    {
        Task<List<Exercise>> GetAllAsync();
        Task<Exercise> GetByIdAsync(string id);
        Task<Exercise> CreateAsync(Exercise exercise);
        Task<Exercise> UpdateAsync(string id, Exercise exercise);
        Task<Exercise?> DeleteAsync(string id);
        
    }
}