using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(AppUser appuser);
        RefreshToken CreateRefreshToken(string ipAddress);
    }
}