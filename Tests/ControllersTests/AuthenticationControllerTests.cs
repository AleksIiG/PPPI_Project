using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Controllers;
using api.Dto.UserDTOs;
using api.Interface;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using FluentAssertions;

namespace Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<IAuthenticationService> _authServiceMock;
        private readonly Mock<IRevorkedTokenService> _revokedTokenServiceMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly AuthenticationController _controller;

        public AuthenticationControllerTests()
        {
            // Створюємо mock-об'єкти для залежностей
            _authServiceMock = new Mock<IAuthenticationService>();
            _revokedTokenServiceMock = new Mock<IRevorkedTokenService>();
            _userRepoMock = new Mock<IUserRepository>();

            // Створюємо контролер з mock-залежностями
            _controller = new AuthenticationController(
                _authServiceMock.Object,
                _userRepoMock.Object,
                _revokedTokenServiceMock.Object
            );

            // Налаштовуємо HTTP контекст з IP-адресою
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Fact]
        // Перевіряє успішну реєстрацію нового користувача
        public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
        {
            // Підготовка
            var registerDto = new RegisterUserDto { Email = "test@test.com", Password = "123456" };
            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<api.Models.AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(("access_token", new api.Models.RefreshToken { Token = "refresh_token" }));

            // Виконання
            var result = await _controller.Register(registerDto);

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє помилку при невалідних даних реєстрації
        public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка невалідних даних
            var registerDto = new RegisterUserDto { Email = "invalid-email", Password = "123" };
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Виконання
            var result = await _controller.Register(registerDto);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє конфлікт при спробі зареєструвати існуючого користувача
        public async Task Register_ShouldReturnConflict_WhenUserAlreadyExists()
        {
            // Підготовка даних для існуючого користувача
            var registerDto = new RegisterUserDto { Email = "exists@test.com", Password = "123456" };
            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<api.Models.AppUser>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("User already exists"));

            // Виконання
            var result = await _controller.Register(registerDto);

            // Перевірка
            var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflict.Value.Should().BeEquivalentTo(new { message = "User already exists" });
        }

        [Fact]
        // Перевіряє успішний вхід з валідними обліковими даними
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            // Підготовка валідних даних
            var loginDto = new LoginUserDto { Email = "test@test.com", Password = "123456" };
            _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<api.Models.AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(("access_token", new api.Models.RefreshToken { Token = "refresh_token" }));

            // Виконання
            var result = await _controller.Login(loginDto);

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє помилку при невалідних даних входу
        public async Task Login_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка невалідних даних
            var loginDto = new LoginUserDto { Email = "", Password = "" };
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Виконання
            var result = await _controller.Login(loginDto);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє відмову в доступі при невірних облікових даних
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsInvalid()
        {
            // Підготовка невірних даних
            var loginDto = new LoginUserDto { Email = "wrong@test.com", Password = "badpass" };
            _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<api.Models.AppUser>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Виконання
            var result = await _controller.Login(loginDto);

            // Перевірка
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { message = "Invalid credentials" });
        }

        [Fact]
        // Перевіряє помилку при відсутності refresh token
        public async Task RefreshToken_ShouldReturnBadRequest_WhenTokenMissing()
        {
            // Підготовка DTO без токена
            var dto = new RefreshTokenDto { RefreshToken = "" };

            // Виконання
            var result = await _controller.RefreshToken(dto);

            // Перевірка
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { message = "Refresh token is required." });
        }

        [Fact]
        // Перевіряє успішне оновлення токенів
        public async Task RefreshToken_ShouldReturnOk_WhenValidTokenProvided()
        {
            // Підготовка валідного токена
            var dto = new RefreshTokenDto { RefreshToken = "valid_token" };
            _authServiceMock.Setup(x => x.RefreshTokenAsync("valid_token", "127.0.0.1"))
                .ReturnsAsync(("new_access", new api.Models.RefreshToken { Token = "new_refresh" }));

            // Виконання
            var result = await _controller.RefreshToken(dto);

            // Перевірка
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє успішний вихід з системи
        public async Task Logout_ShouldReturnOk_WhenLogoutSuccessful()
        {
            // Підготовка токена для виходу
            var logoutDto = new LogoutDto { RefreshToken = "refresh_token" };
            var token = CreateTestJwtToken("jti_123");
            SetupHttpContextWithBearerToken(token);

            // Виконання
            var result = await _controller.Logout(logoutDto);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє успішний вихід з усіх пристроїв
        public async Task LogoutAll_ShouldReturnOk_WhenLogoutAllSuccessful()
        {
            // Підготовка автентифікованого користувача
            var userId = "user123";
            SetupUserClaims(userId);

            // Виконання
            var result = await _controller.LogoutAll();

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при оновленні неіснуючого токена
        public async Task RefreshToken_ShouldReturnNotFound_WhenTokenNotFound()
        {
            // Підготовка неіснуючого токена
            var dto = new RefreshTokenDto { RefreshToken = "not_found_token" };
            _authServiceMock.Setup(x => x.RefreshTokenAsync("not_found_token", "127.0.0.1"))
                .ThrowsAsync(new KeyNotFoundException("Token not found"));

            // Виконання
            var result = await _controller.RefreshToken(dto);

            // Перевірка
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().BeEquivalentTo(new { message = "Token not found" });
        }

        [Fact]
        // Перевіряє відмову в доступі при невалідному токені
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsInvalid()
        {
            // Підготовка невалідного токена
            var dto = new RefreshTokenDto { RefreshToken = "invalid_token" };
            _authServiceMock.Setup(x => x.RefreshTokenAsync("invalid_token", "127.0.0.1"))
                .ThrowsAsync(new UnauthorizedAccessException("Token is invalid"));

            // Виконання
            var result = await _controller.RefreshToken(dto);

            // Перевірка
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { message = "Token is invalid" });
        }

        [Fact]
        // Перевіряє помилку при відсутності авторизації для виходу
        public async Task Logout_ShouldReturnUnauthorized_WhenAuthorizationHeaderMissing()
        {
            // Підготовка даних без заголовка авторизації
            var logoutDto = new LogoutDto { RefreshToken = "refresh_token" };
            _controller.ControllerContext.HttpContext.Request.Headers.Remove("Authorization");

            // Виконання
            var result = await _controller.Logout(logoutDto);

            // Перевірка
            result.Should().BeOfType<UnauthorizedObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new { message = "Authorization header missing or invalid." });
        }

        [Fact]
        // Перевіряє помилку при відсутності автентифікації для виходу з усіх пристроїв
        public async Task LogoutAll_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Підготовка неавтентифікованого користувача
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Виконання
            var result = await _controller.LogoutAll();

            // Перевірка
            result.Should().BeOfType<UnauthorizedObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new { message = "User not authenticated." });
        }

        // Допоміжні методи для тестів
        private string CreateTestJwtToken(string jti)
        {
            // Створює тестовий JWT токен
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(jti))
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, "test_user"));

            var token = new JwtSecurityToken(
                issuer: "test_issuer",
                audience: "test_audience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("super_long_test_secret_key_12345")),
            SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void SetupHttpContextWithBearerToken(string token)
        {
            // Додає токен в заголовок авторизації
            _controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        private void SetupUserClaims(string userId)
        {
            // Створює claims для імітації автентифікації
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
        }
    }
}
