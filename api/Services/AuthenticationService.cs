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

            var createdUser = await _userRepo.CreateAsync(appUser);

            var token = _tokenService.CreateAccessToken(createdUser);
            var RefreshToken = _tokenService.CreateRefreshToken(ipAddress);

            createdUser.RefreshTokens.Add(RefreshToken);



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


        public async Task<(string token, RefreshToken RefreshToken)> LoginAsync(AppUser appUser, string ipAddress)
        {
            var existingUser = await _userRepo.GetByEmailAsync(appUser.Email);
            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(appUser.PasswordHash, existingUser.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var token = _tokenService.CreateAccessToken(existingUser);
            var RefreshToken = _tokenService.CreateRefreshToken(ipAddress);

            existingUser.RefreshTokens.Add(RefreshToken);
            _tokenService.RemoveRefreshTokens(appUser);
            await _userRepo.UpdateAsync(existingUser.Id, existingUser);

            return (token, RefreshToken);
        }

        public async Task<(string token, RefreshToken RefreshToken)> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var appUser = await _userRepo.GetByRefreshAsync(refreshToken);
            if (appUser == null)
            {
                throw new KeyNotFoundException("Invalid refresh token.");
            }

            var existingToken = appUser.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken && !t.IsRevoked);
            if (existingToken == null || existingToken.Expires <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
            }

            var newAccessToken = _tokenService.CreateAccessToken(appUser);
            var newRefreshToken = _tokenService.CreateRefreshToken(ipAddress);

            existingToken.IsRevoked = true;
            existingToken.Revoked = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;

            _tokenService.RemoveRefreshTokens(appUser);

            appUser.RefreshTokens.Add(newRefreshToken);
            await _userRepo.UpdateAsync(appUser.Id, appUser);


            return (newAccessToken, newRefreshToken);
        }


        public async Task LogoutAllAsync(string userId)
        {
            var existingUser = await _userRepo.GetByIdAsync(userId);
            if (existingUser == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            _tokenService.RemoveALLRefreshTokens(existingUser);

            var result = await _userRepo.UpdateAsync(existingUser.Id, existingUser);
            if (result == null)
            {
                throw new Exception("Failed to update user during logout all.");
            }
        }

    }
}