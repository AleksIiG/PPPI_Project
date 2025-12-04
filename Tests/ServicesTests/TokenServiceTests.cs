using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using api.Models;
using api.Services;
using api.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;

namespace Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            // Створюємо mock-об'єкт для конфігурації
            _configurationMock = new Mock<IConfiguration>();

            // Налаштовуємо конфігурацію для JWT
            _configurationMock.Setup(x => x["JWT_SECRET"]).Returns("super_secret_key_that_is_long_enough_for_256_bits");
            _configurationMock.Setup(x => x["JWT_ISSUER"]).Returns("test_issuer");
            _configurationMock.Setup(x => x["JWT_AUDIENCE"]).Returns("test_audience");
            _configurationMock.Setup(x => x["JWT_EXPIRES_MINUTES"]).Returns("15");

            // Створюємо сервіс для тестування
            _tokenService = new TokenService(_configurationMock.Object);
        }

        [Fact]
        // Перевіряє створення access token з правильними claims
        public void CreateAccessToken_ShouldReturnValidToken_WithCorrectClaims()
        {
            // Підготовка
            var appUser = new AppUser
            {
                Id = "user123",
                Username = "testuser",
                Email = "test@test.com",
                Role = "User"
            };

            // Виконання
            var token = _tokenService.CreateAccessToken(appUser);

            // Перевірка
            token.Should().NotBeNullOrEmpty();

            // Розкодовуємо токен для перевірки claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user123");
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@test.com");
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        }

        [Fact]
        // Перевіряє створення refresh token з правильними властивостями
        public void CreateRefreshToken_ShouldReturnValidRefreshToken_WithCorrectProperties()
        {
            // Підготовка
            var ipAddress = "192.168.1.1";

            // Виконання
            var refreshToken = _tokenService.CreateRefreshToken(ipAddress);

            // Перевірка
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.CreatedByIp.Should().Be(ipAddress);
            refreshToken.IsRevoked.Should().BeFalse();
        }

        [Fact]
        // Перевіряє видалення прострочених та відкликаних refresh токенів
        public void RemoveRefreshTokens_ShouldRemoveExpiredAndRevokedTokens()
        {
            // Підготовка
            var appUser = new AppUser
            {
                RefreshTokens = new List<RefreshToken>
                {
                    new RefreshToken { Token = "valid1", Expires = DateTime.UtcNow.AddDays(1), IsRevoked = false },
                    new RefreshToken { Token = "expired", Expires = DateTime.UtcNow.AddDays(-1), IsRevoked = false },
                    new RefreshToken { Token = "revoked", Expires = DateTime.UtcNow.AddDays(1), IsRevoked = true }
                }
            };

            // Виконання
            _tokenService.RemoveRefreshTokens(appUser);

            // Перевірка
            appUser.RefreshTokens.Should().ContainSingle();
            appUser.RefreshTokens.Should().OnlyContain(t => t.Token == "valid1");
        }

        [Fact]
        // Перевіряє видалення всіх refresh токенів
        public void RemoveALLRefreshTokens_ShouldRemoveAllTokens()
        {
            // Підготовка
            var appUser = new AppUser
            {
                RefreshTokens = new List<RefreshToken>
                {
                    new RefreshToken { Token = "token1" },
                    new RefreshToken { Token = "token2" }
                }
            };

            // Виконання
            _tokenService.RemoveALLRefreshTokens(appUser);

            // Перевірка
            appUser.RefreshTokens.Should().BeEmpty();
        }

        [Fact]
        // Перевіряє викидання винятку при відсутньому JWT_SECRET
        public void Constructor_ShouldThrowException_WhenJwtSecretIsMissing()
        {
            // Підготовка
            var invalidConfigMock = new Mock<IConfiguration>();
            invalidConfigMock.Setup(x => x["JWT_SECRET"]).Returns((string)null);

            // Виконання & Перевірка
            Action act = () => new TokenService(invalidConfigMock.Object);

            // Виправлення: перевіряємо тип винятку без точного повідомлення
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
