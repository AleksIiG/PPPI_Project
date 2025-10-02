using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interface;
using api.Models;
using api.Services.Interfaces;

namespace api.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUSerService _userService;
        private readonly ITokenService _tokenService;


        public AuthenticationService(IUserRepository userRepo, IUSerService userService, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<(string token, RefreshToken RefreshToken)> RegisterAsync(AppUser appUser, string ipAddress)
        {
            var existingUser = await _userService.UserExistsAsync(appUser);
            if (existingUser)
            {
                throw new InvalidOperationException($"User with email {appUser.Email} already exists.");
            }

            var token = _tokenService.CreateAccessToken(appUser);
            var RefreshToken = _tokenService.CreateRefreshToken(ipAddress);

            appUser.RefreshTokens.Add(RefreshToken);
            await _userRepo.CreateAsync(appUser);



            return (token, RefreshToken);

        }

        public async Task LogoutByRefreshToken(string refreshToken, string ipAddress)
        {
            var appUser = await _userRepo.GetByRefreshAsync(refreshToken);
            if (appUser == null)
            {
                throw new KeyNotFoundException("Invalid refresh token.");
            }

            var token = appUser.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken && !t.IsRevoked);
            if (token != null)
            {
                token.IsRevoked = true;
                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;

                var result = await _userRepo.UpdateAsync(appUser.Id, appUser)
                    ?? throw new Exception("Failed to update user during logout.");
            }
        }

        
    }
}