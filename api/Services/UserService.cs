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
    }
}