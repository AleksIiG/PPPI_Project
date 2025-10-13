using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Models;
using api.Services.Interfaces;
using MongoDB.Driver;

namespace api.Services
{
    public class RevorkedTokenService : IRevorkedTokenService
    {
        private readonly MongoDbService _database;
        private readonly int _expriresAt;

        public RevorkedTokenService(MongoDbService db, IConfiguration config)
        {
            _database = db;
            _expriresAt = int.Parse(config["JWT_EXPIRES_MINUTES"] ?? "15");
        }


        public async Task<bool> IsRevokedAsync(string jti)
        {
            return await _database.RevorkedTokens.Find(rt => rt.Jti == jti).AnyAsync();
        }

        public async Task RevokeAsync(string jti)
        {
            var existing = await _database.RevorkedTokens.Find(rt => rt.Jti == jti).FirstOrDefaultAsync();
            if (existing != null)
            {
                // Токен вже відкликаний
                return;
            }
            var doc = new RevorkedToken
            {
                Jti = jti,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_expriresAt)
            };
            await _database.RevorkedTokens.InsertOneAsync(doc);
        }


    }
}