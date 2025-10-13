using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services.Interfaces
{
    public interface IRevorkedTokenService
    {
        Task RevokeAsync(string jti);
        Task<bool> IsRevokedAsync(string jti);
    }
}