using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using api.Interface;
using api.Models;
using api.Services.Interfaces;

namespace api.Services
{
    public class UserService : IUSerService
    {
        private readonly IUserRepository _userRepo;
        private readonly IWorkoutRepository _workoutRepo;
        public UserService(IUserRepository userRepo, IWorkoutRepository workoutRepo)
        {
            _userRepo = userRepo;
            _workoutRepo = workoutRepo;
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

        public async Task<bool> WorkoutToFavoritesAsync(string userId, string workoutId)
        {
            bool wasAdded = false;
            var user = await _userRepo.GetByIdAsync(userId) ?? throw new KeyNotFoundException($"User with id {userId} not found.");
            var workout = await _workoutRepo.GetByIdAsync(workoutId) ?? throw new KeyNotFoundException($"Workout with id {workoutId} not found.");

            if (user.LikedWorkouts.Contains(workoutId))
            {
                user.LikedWorkouts.Remove(workoutId);
            }
            else
            {
                user.LikedWorkouts.Add(workoutId);
                wasAdded = true;
            }

            await _userRepo.UpdateAsync(userId, user);
            return wasAdded;
        }



    }
}