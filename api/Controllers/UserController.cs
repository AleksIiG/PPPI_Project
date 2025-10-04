using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using api.Data;
using api.Dto.UserDTOs;
using api.Interface;
using api.Mappers.UserMapper;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Driver;

namespace api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUSerService _userService;
        public UserController(IUSerService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUser()
        {
            var id = User.FindFirst("id")?.Value;
            if (id == null) return Unauthorized(new { message = "User ID not found in token." });
            try
            {
                var user = await _userService.GetCurrentUserByIdAsync(id);
                return Ok(user.ToUserDto());
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { message = ex.Message }) { StatusCode = 500 };
            }
        }
    }
}