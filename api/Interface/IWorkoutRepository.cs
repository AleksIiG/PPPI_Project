using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerciseDTOs;
using api.Models;

namespace api.Interface
{
    public interface IWorkoutRepository
    {
        Task<List<Workout>> GetAllAsync();
        Task<Workout?> GetByIdAsync(string id);
        Task<Workout> CreateAsync(Workout workoutModel);
        Task<Workout?> UpdateAsync(string id, Workout workoutModel);
        Task<Workout?> DeleteAsync(string id);
        Task<bool> ExistsByNameAsync(string name);
    }
}
