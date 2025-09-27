using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dto.ExerciseDTOs;
using api.Interface;
using api.Models;
using MongoDB.Driver;

namespace api.Repesitory
{
    public class UserRepository : IUserRepository
    {

        private readonly MongoDbService _database;

        public UserRepository(MongoDbService database)
        {
            _database = database;
        }

        public async Task<AppUser> CreateAsync(AppUser appUser)
        {
            await _database.Users.InsertOneAsync(appUser);
            return appUser;
        }

        public async Task<AppUser?> DeleteAsync(string id)
        {
            var appUser = await _database.Users.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (appUser == null)
            {
                return null;
            }
            return appUser;
        }

        public async Task<List<AppUser>> GetAllAsync()
        {
            return await _database.Users.Find(_ => true).ToListAsync();
        }

        public async Task<AppUser?> GetByIdAsync(string id)
        {
            return await _database.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<AppUser?> UpdateAsync(string Id, AppUser appUser)
        {
            var result = await _database.Users.ReplaceOneAsync(e => e.Id == Id, appUser);
            if (result == null)
            {
                return null;
            }
            return appUser;
        }
    }
}
