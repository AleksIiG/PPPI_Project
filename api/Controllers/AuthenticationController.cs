using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dto.UserDTOs;
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
        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
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
            catch (InvalidOperationException  ex)
            {
                return Conflict(new { message = ex.Message });
            }

            

            
        }
    }
}