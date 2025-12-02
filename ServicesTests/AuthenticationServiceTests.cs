using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interface;
using api.Models;
using api.Services;
using api.Services.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;

namespace Tests.Services
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUSerService> _userServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            // Створюємо mock-об'єкти для залежностей сервісу автентифікації
            _userRepoMock = new Mock<IUserRepository>();
            _userServiceMock = new Mock<IUSerService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _authService = new AuthenticationService(_userRepoMock.Object, _userServiceMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        // Перевіряє успішну реєстрацію нового користувача
        public async Task RegisterAsync_ShouldReturnTokens_WhenUserDoesNotExist()
        {
            // Підготовка
            var appUser = new AppUser { Email = "test@test.com", PasswordHash = "password" };
            var ipAddress = "127.0.0.1";
            var expectedToken = "access_token";
            var expectedRefreshToken = new RefreshToken { Token = "refresh_token" };

            _userServiceMock.Setup(x => x.UserExistsAsync(appUser)).ReturnsAsync(false);
            _tokenServiceMock.Setup(x => x.CreateAccessToken(appUser)).Returns(expectedToken);
            _tokenServiceMock.Setup(x => x.CreateRefreshToken(ipAddress)).Returns(expectedRefreshToken);
            _userRepoMock.Setup(x => x.CreateAsync(appUser)).ReturnsAsync(appUser);

            // Виконання
            var result = await _authService.RegisterAsync(appUser, ipAddress);

            // Перевірка
            result.token.Should().Be(expectedToken);
            result.RefreshToken.Should().Be(expectedRefreshToken);
            _userServiceMock.Verify(x => x.UserExistsAsync(appUser), Times.Once);
            _tokenServiceMock.Verify(x => x.CreateAccessToken(appUser), Times.Once);
            _tokenServiceMock.Verify(x => x.CreateRefreshToken(ipAddress), Times.Once);
            _userRepoMock.Verify(x => x.CreateAsync(appUser), Times.Once);
        }

        [Fact]
        // Перевіряє помилку при спробі зареєструвати існуючого користувача
        public async Task RegisterAsync_ShouldThrowException_WhenUserAlreadyExists()
        {
            // Підготовка
            var appUser = new AppUser { Email = "existing@test.com", PasswordHash = "password" };
            var ipAddress = "127.0.0.1";

            _userServiceMock.Setup(x => x.UserExistsAsync(appUser)).ReturnsAsync(true);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(appUser, ipAddress)
            );
            exception.Message.Should().Be($"User with email {appUser.Email} already exists.");
        }

        [Fact]
        // Перевіряє успішний вхід з валідними обліковими даними
        public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
        {
            // Підготовка
            var appUser = new AppUser { Email = "test@test.com", PasswordHash = "password" };
            var existingUser = new AppUser
            {
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                RefreshTokens = new List<RefreshToken>()
            };
            var ipAddress = "127.0.0.1";
            var expectedToken = "access_token";
            var expectedRefreshToken = new RefreshToken { Token = "refresh_token" };

            _userRepoMock.Setup(x => x.GetByEmailAsync(appUser.Email)).ReturnsAsync(existingUser);
            _tokenServiceMock.Setup(x => x.CreateAccessToken(existingUser)).Returns(expectedToken);
            _tokenServiceMock.Setup(x => x.CreateRefreshToken(ipAddress)).Returns(expectedRefreshToken);
            _tokenServiceMock.Setup(x => x.RemoveRefreshTokens(It.IsAny<AppUser>()));
            _userRepoMock.Setup(x => x.UpdateAsync(existingUser.Id, existingUser)).ReturnsAsync(existingUser);

            // Виконання
            var result = await _authService.LoginAsync(appUser, ipAddress);

            // Перевірка
            result.token.Should().Be(expectedToken);
            result.RefreshToken.Should().Be(expectedRefreshToken);
            _userRepoMock.Verify(x => x.GetByEmailAsync(appUser.Email), Times.Once);
            _tokenServiceMock.Verify(x => x.CreateAccessToken(existingUser), Times.Once);
            _tokenServiceMock.Verify(x => x.CreateRefreshToken(ipAddress), Times.Once);
            _userRepoMock.Verify(x => x.UpdateAsync(existingUser.Id, existingUser), Times.Once);
        }

        [Fact]
        // Перевіряє помилку при спробі входу з неіснуючим email
        public async Task LoginAsync_ShouldThrowException_WhenUserNotFound()
        {
            // Підготовка
            var appUser = new AppUser { Email = "nonexistent@test.com", PasswordHash = "password" };
            var ipAddress = "127.0.0.1";

            _userRepoMock.Setup(x => x.GetByEmailAsync(appUser.Email)).ReturnsAsync((AppUser)null);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(appUser, ipAddress)
            );
            exception.Message.Should().Be("Invalid email or password.");
        }

        [Fact]
        // Перевіряє помилку при спробі входу з невірним паролем
        public async Task LoginAsync_ShouldThrowException_WhenPasswordIsInvalid()
        {
            // Підготовка
            var appUser = new AppUser { Email = "test@test.com", PasswordHash = "wrong_password" };
            var existingUser = new AppUser
            {
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct_password")
            };
            var ipAddress = "127.0.0.1";

            _userRepoMock.Setup(x => x.GetByEmailAsync(appUser.Email)).ReturnsAsync(existingUser);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(appUser, ipAddress)
            );
            exception.Message.Should().Be("Invalid email or password.");
        }

        [Fact]
        // Перевіряє успішне оновлення токенів за допомогою валідного refresh token
        public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Підготовка
            var refreshToken = "valid_refresh_token";
            var ipAddress = "127.0.0.1";
            var existingToken = new RefreshToken
            {
                Token = refreshToken,
                IsRevoked = false,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            var appUser = new AppUser
            {
                Id = "1",
                RefreshTokens = new List<RefreshToken> { existingToken }
            };
            var newAccessToken = "new_access_token";
            var newRefreshToken = new RefreshToken { Token = "new_refresh_token" };

            _userRepoMock.Setup(x => x.GetByRefreshAsync(refreshToken)).ReturnsAsync(appUser);
            _tokenServiceMock.Setup(x => x.CreateAccessToken(appUser)).Returns(newAccessToken);
            _tokenServiceMock.Setup(x => x.CreateRefreshToken(ipAddress)).Returns(newRefreshToken);
            _tokenServiceMock.Setup(x => x.RemoveRefreshTokens(appUser));
            _userRepoMock.Setup(x => x.UpdateAsync(appUser.Id, appUser)).ReturnsAsync(appUser);

            // Виконання
            var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

            // Перевірка
            result.token.Should().Be(newAccessToken);
            result.RefreshToken.Should().Be(newRefreshToken);
            existingToken.IsRevoked.Should().BeTrue();
            _userRepoMock.Verify(x => x.GetByRefreshAsync(refreshToken), Times.Once);
            _tokenServiceMock.Verify(x => x.CreateAccessToken(appUser), Times.Once);
            _tokenServiceMock.Verify(x => x.CreateRefreshToken(ipAddress), Times.Once);
            _userRepoMock.Verify(x => x.UpdateAsync(appUser.Id, appUser), Times.Once);
        }

        [Fact]
        // Перевіряє помилку при оновленні токенів з неіснуючим refresh token
        public async Task RefreshTokenAsync_ShouldThrowException_WhenRefreshTokenNotFound()
        {
            // Підготовка
            var refreshToken = "invalid_refresh_token";
            var ipAddress = "127.0.0.1";

            _userRepoMock.Setup(x => x.GetByRefreshAsync(refreshToken)).ReturnsAsync((AppUser)null);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _authService.RefreshTokenAsync(refreshToken, ipAddress)
            );
            exception.Message.Should().Be("Invalid refresh token.");
        }

        [Fact]
        // Перевіряє помилку при оновленні токенів з простроченим refresh token
        public async Task RefreshTokenAsync_ShouldThrowException_WhenRefreshTokenIsExpired()
        {
            // Підготовка
            var refreshToken = "expired_refresh_token";
            var ipAddress = "127.0.0.1";
            var expiredToken = new RefreshToken
            {
                Token = refreshToken,
                IsRevoked = false,
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            var appUser = new AppUser
            {
                RefreshTokens = new List<RefreshToken> { expiredToken }
            };

            _userRepoMock.Setup(x => x.GetByRefreshAsync(refreshToken)).ReturnsAsync(appUser);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.RefreshTokenAsync(refreshToken, ipAddress)
            );
            exception.Message.Should().Be("Refresh token is expired or revoked.");
        }

        [Fact]
        // Перевіряє помилку при оновленні токенів з відкликаним refresh token
        public async Task RefreshTokenAsync_ShouldThrowException_WhenRefreshTokenIsRevoked()
        {
            // Підготовка
            var refreshToken = "revoked_refresh_token";
            var ipAddress = "127.0.0.1";
            var revokedToken = new RefreshToken
            {
                Token = refreshToken,
                IsRevoked = true,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            var appUser = new AppUser
            {
                RefreshTokens = new List<RefreshToken> { revokedToken }
            };

            _userRepoMock.Setup(x => x.GetByRefreshAsync(refreshToken)).ReturnsAsync(appUser);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.RefreshTokenAsync(refreshToken, ipAddress)
            );
            exception.Message.Should().Be("Refresh token is expired or revoked.");
        }

        [Fact]
        // Перевіряє успішний вихід з відкликанням refresh token
        public async Task LogoutByRefreshToken_ShouldRevokeToken_WhenTokenExists()
        {
            // Підготовка
            var refreshToken = "valid_refresh_token";
            var ipAddress = "127.0.0.1";
            var token = new RefreshToken
            {
                Token = refreshToken,
                IsRevoked = false
            };
            var appUser = new AppUser
            {
                Id = "1",
                RefreshTokens = new List<RefreshToken> { token }
            };

            _userRepoMock.Setup(x => x.GetByRefreshAsync(refreshToken)).ReturnsAsync(appUser);
            _userRepoMock.Setup(x => x.UpdateAsync(appUser.Id, appUser)).ReturnsAsync(appUser);

            // Виконання
            await _authService.LogoutByRefreshToken(refreshToken, ipAddress);

            // Перевірка
            token.IsRevoked.Should().BeTrue();
            token.Revoked?.Date.Should().Be(DateTime.UtcNow.Date);
            token.RevokedByIp.Should().Be(ipAddress);
            _userRepoMock.Verify(x => x.GetByRefreshAsync(refreshToken), Times.Once);
            _userRepoMock.Verify(x => x.UpdateAsync(appUser.Id, appUser), Times.Once);
        }

        [Fact]
        // Перевіряє помилку при спробі виходу з неіснуючим refresh token
        public async Task LogoutByRefreshToken_ShouldThrowException_WhenTokenNotFound()
        {
            // Підготовка
            var refreshToken = "invalid_refresh_token";
            var ipAddress = "127.0.0.1";

            _userRepoMock.Setup(x => x.GetByRefreshAsync(refreshToken)).ReturnsAsync((AppUser)null);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _authService.LogoutByRefreshToken(refreshToken, ipAddress)
            );
            exception.Message.Should().Be("Invalid refresh token.");
        }

        [Fact]
        // Перевіряє успішний вихід з усіх пристроїв
        public async Task LogoutAllAsync_ShouldRemoveAllRefreshTokens_WhenUserExists()
        {
            // Підготовка
            var userId = "1";
            var appUser = new AppUser
            {
                Id = userId,
                RefreshTokens = new List<RefreshToken>
                {
                    new RefreshToken { Token = "token1" },
                    new RefreshToken { Token = "token2" }
                }
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(appUser);
            _tokenServiceMock.Setup(x => x.RemoveALLRefreshTokens(appUser));
            _userRepoMock.Setup(x => x.UpdateAsync(userId, appUser)).ReturnsAsync(appUser);

            // Виконання
            await _authService.LogoutAllAsync(userId);

            // Перевірка
            _userRepoMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _tokenServiceMock.Verify(x => x.RemoveALLRefreshTokens(appUser), Times.Once);
            _userRepoMock.Verify(x => x.UpdateAsync(userId, appUser), Times.Once);
        }

        [Fact]
        // Перевіряє помилку при спробі виходу з усіх пристроїв для неіснуючого користувача
        public async Task LogoutAllAsync_ShouldThrowException_WhenUserNotFound()
        {
            // Підготовка
            var userId = "nonexistent";

            _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((AppUser)null);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _authService.LogoutAllAsync(userId)
            );
            exception.Message.Should().Be("User not found.");
        }

        [Fact]
        // Перевіряє помилку при невдалому оновленні користувача під час виходу з усіх пристроїв
        public async Task LogoutAllAsync_ShouldThrowException_WhenUpdateFails()
        {
            // Підготовка
            var userId = "1";
            var appUser = new AppUser { Id = userId, RefreshTokens = new List<RefreshToken>() };

            _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(appUser);
            _tokenServiceMock.Setup(x => x.RemoveALLRefreshTokens(appUser));
            _userRepoMock.Setup(x => x.UpdateAsync(userId, appUser)).ReturnsAsync((AppUser)null);

            // Виконання & Перевірка
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.LogoutAllAsync(userId)
            );
            exception.Message.Should().Be("Failed to update user during logout all.");
        }
    }
}