using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<(string token, RefreshToken RefreshToken)> RegisterAsync(AppUser appUser, string ipAddress);
        public Task LogoutByRefreshToken(string refreshToken, string ipAddress);
    }
}