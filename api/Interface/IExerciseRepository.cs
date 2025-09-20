using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interface
{
    public interface IExerciseRepository
    {
        Task<List<Exercise>> GetAllAsync();
        Task<Exercise?> GetByIdAsync(string id);
        Task<Exercise?> DeleteAsync (string id);
        Task<Exercise?> Create(Exercise commentModel);
        // Task<Exercise?> Update(string Id, Exercise commentDto);
    }
}