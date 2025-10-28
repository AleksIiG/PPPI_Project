using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dto.UserDTOs;
using api.Interface;
using api.Mappers.UserMapper;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    /// <summary>
    /// Контролер для аутентифікації користувачів (реєстрація, вхід, вихід, оновлення токена).
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IRevorkedTokenService _revorkedTokenService;

        /// <summary>
        /// Створює новий екземпляр контролера автентифікації.
        /// </summary>
        /// <param name="authService">Сервіс для автентифікації користувачів.</param>
        /// <param name="userRepo">Репозиторій користувачів (не використовується напряму).</param>
        /// <param name="revorkedTokenService">Сервіс для роботи з відкликаними токенами.</param>
        public AuthenticationController(
            IAuthenticationService authService,
            IUserRepository userRepo,
            IRevorkedTokenService revorkedTokenService)
        {
            _authService = authService;
            _revorkedTokenService = revorkedTokenService;
        }

        /// <summary>
        /// Реєструє нового користувача та повертає JWT-токени.
        /// </summary>
        /// <param name="registerUserDto">Дані для реєстрації користувача.</param>
        /// <returns>JWT-токен доступу та refresh-токен.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        /// <summary>
        /// Виконує вхід користувача за логіном і паролем.
        /// </summary>
        /// <param name="loginUserDto">Дані для входу користувача.</param>
        /// <returns>JWT-токен доступу та refresh-токен.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(
                    loginUserDto.ToAppUserFromLoginDto(),
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

                return Ok(new
                {
                    accessToken = result.token,
                    refreshToken = result.RefreshToken.Token
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Оновлює пару токенів за допомогою дійсного refresh-токена.
        /// </summary>
        /// <param name="refreshTokenDto">Об’єкт, що містить refresh-токен.</param>
        /// <returns>Нова пара токенів (access і refresh).</returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
                return BadRequest(new { message = "Refresh token is required." });

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken, ipAddress);
                return Ok(new
                {
                    accessToken = result.token,
                    refreshToken = result.RefreshToken.Token
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Вихід користувача (відкликає поточний токен).
        /// </summary>
        /// <param name="logoutDto">Об’єкт із refresh-токеном користувача.</param>
        /// <returns>Підтвердження виходу.</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { message = "Authorization header missing or invalid." });

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
                return BadRequest(new { message = "Token does not contain JTI." });

            await _revorkedTokenService.RevokeAsync(jti);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await _authService.LogoutByRefreshToken(logoutDto.RefreshToken, ipAddress);

            return Ok(new { message = "Logout successful." });
        }

        /// <summary>
        /// Вихід з усіх пристроїв (відкликає всі токени користувача).
        /// </summary>
        /// <returns>Підтвердження виходу з усіх пристроїв.</returns>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            await _authService.LogoutAllAsync(userId);

            return Ok(new { message = "Logged out from all devices." });
        }
    }
}
