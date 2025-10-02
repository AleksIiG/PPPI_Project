using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using api.Data;
using api.Dto.UserDTOs;
using api.Interface;
using api.Mappers.UserMapper;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Driver;

namespace api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserRepository _userRepo;
        public AuthenticationController(IAuthenticationService authService, IUserRepository userRepo) 
        {
            _authService = authService;
            _userRepo = userRepo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            try
            {
                var result = await _authService.RegisterAsync(registerUserDto.ToAppUserFromRegisterDto(), ipAddress);
                return Ok(new
                {
                    accessToken = result.token,
                    refreshToken = result.RefreshToken.Token
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            try
            {
                if (string.IsNullOrEmpty(logoutDto.RefreshToken))
                {
                    return BadRequest(new { message = "Refresh token is required." });
                }
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                await _authService.LogoutByRefreshToken(logoutDto.RefreshToken, ipAddress);
                return Ok(new { message = "Logout successful." } );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}