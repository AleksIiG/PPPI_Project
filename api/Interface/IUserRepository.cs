using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using api.Dto.ExerciseDTOs;
using api.Models;

namespace api.Interface
{
    public interface IUserRepository
    {
        Task<List<AppUser>> GetAllAsync();

        Task<AppUser?> GetByIdAsync(string id);
        Task<AppUser?> DeleteAsync(string id);
        Task<AppUser> CreateAsync(AppUser appUser);
        Task<AppUser?> UpdateAsync(string Id, AppUser appUser);

    }
}