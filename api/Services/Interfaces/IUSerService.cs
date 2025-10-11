using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Services.Interfaces
{
    public interface IUSerService
    {
        public Task<bool> UserExistsAsync(AppUser appUser);
        public Task<AppUser> GetCurrentUserByIdAsync(string id);
        public Task<List<AppUser>> GetAllUsersAsync();
        public Task UpdateUserASync(string id, AppUser user);
    }
}