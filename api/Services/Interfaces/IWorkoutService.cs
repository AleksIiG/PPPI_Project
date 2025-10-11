using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerTagsDto;
using api.Helpers;
using api.Models;

namespace api.Services.Interfaces
{
    public interface IWorkoutService
    {
        Task<List<Workout>> GetAllAsync(QueryObjectForWorkouts query);
        Task<Workout> GetByIdAsync(string id);
        Task<Workout> CreateAsync(Workout workout);
        Task<Workout> UpdateAsync(string id, Workout workout);
        Task<Workout?> DeleteAsync(string id);
    }
}
