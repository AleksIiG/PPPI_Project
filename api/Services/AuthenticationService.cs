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
        public AuthenticationService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public Task<string> Register(AppUser appuser)
        {
            throw new NotImplementedException();
        }
    }
}