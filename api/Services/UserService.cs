using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interface;
using api.Models;
using api.Services.Interfaces;

namespace api.Services
{
    public class UserService : IUSerService
    {
        private readonly IUserRepository _userRepo;
        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<bool> UserExistsAsync(AppUser appUser)
        {
            return await _userRepo.GetByEmailAsync(appUser.Email) != null;
        }

        public async Task<AppUser> GetCurrentUserByIdAsync(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }
            return user;
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _userRepo.GetAllAsync();
        }

        public async Task UpdateUserASync(string id, AppUser user)
        {
            await _userRepo.UpdateAsync(id, user);
        }
    }
}