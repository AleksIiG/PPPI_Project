using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using api.Models;
using api.Services;
using Microsoft.Extensions.Configuration;


namespace Tests
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            // Імітуємо конфігурацію для TokenService
            var inMemorySettings = new Dictionary<string, string> {
                {"JWT_SECRET", "supersecretkey1234567890"},
                {"JWT_ISSUER", "test_issuer"},
                {"JWT_AUDIENCE", "test_audience"},
                {"JWT_EXPIRES_MINUTES", "5"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _tokenService = new TokenService(configuration);
        }

        [Fact]
        public void CreateRefreshToken_ShouldReturnValidToken()
        {
            // Arrange
            var ipAddress = "127.0.0.1";

            // Act
            var refreshToken = _tokenService.CreateRefreshToken(ipAddress);

            // Assert
            Assert.NotNull(refreshToken);
            Assert.False(string.IsNullOrEmpty(refreshToken.Token));
            Assert.Equal(ipAddress, refreshToken.CreatedByIp);
            Assert.False(refreshToken.IsRevoked);
            Assert.True(refreshToken.Expires > DateTime.UtcNow);
        }

        [Fact]
        public void RemoveRefreshTokens_ShouldRemoveExpiredTokens()
        {
            // Arrange
            var appUser = new AppUser
            {
                Id = "1",
                Username = "test",
                Email = "test@example.com",
                RefreshTokens = new List<RefreshToken>
                {
                    new RefreshToken { Token = "expired", Expires = DateTime.UtcNow.AddDays(-1), IsRevoked = false },
                    new RefreshToken { Token = "active", Expires = DateTime.UtcNow.AddDays(1), IsRevoked = false }
                }
            };

            // Act
            _tokenService.RemoveRefreshTokens(appUser);

            // Assert
            Assert.Single(appUser.RefreshTokens); // має залишитись лише активний токен
            Assert.Equal("active", appUser.RefreshTokens[0].Token);
        }
    }
}

