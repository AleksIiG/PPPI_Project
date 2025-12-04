using System;
using System.Collections.Generic;
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
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Створюємо mock-об'єкт для репозиторію користувачів
            _userRepoMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepoMock.Object);
        }

        [Fact]
        // Перевіряє перевірку існування користувача за email
        public async Task UserExistsAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Підготовка
            var appUser = new AppUser { Email = "test@test.com" };
            _userRepoMock.Setup(x => x.GetByEmailAsync(appUser.Email))
                        .ReturnsAsync(new AppUser());

            // Виконання
            var result = await _userService.UserExistsAsync(appUser);

            // Перевірка
            result.Should().BeTrue();
        }

        [Fact]
        // Перевіряє перевірку існування користувача за email
        public async Task UserExistsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Підготовка
            var appUser = new AppUser { Email = "nonexistent@test.com" };
            _userRepoMock.Setup(x => x.GetByEmailAsync(appUser.Email))
                        .ReturnsAsync((AppUser)null);

            // Виконання
            var result = await _userService.UserExistsAsync(appUser);

            // Перевірка
            result.Should().BeFalse();
        }

        [Fact]
        // Перевіряє отримання користувача за існуючим ID
        public async Task GetCurrentUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Підготовка
            var userId = "user123";
            var expectedUser = new AppUser { Id = userId, Email = "test@test.com" };
            _userRepoMock.Setup(x => x.GetByIdAsync(userId))
                        .ReturnsAsync(expectedUser);

            // Виконання
            var result = await _userService.GetCurrentUserByIdAsync(userId);

            // Перевірка
            result.Should().Be(expectedUser);
        }

        [Fact]
        // Перевіряє помилку при отриманні неіснуючого користувача
        public async Task GetCurrentUserByIdAsync_ShouldThrowException_WhenUserNotFound()
        {
            // Підготовка
            var userId = "nonexistent";
            _userRepoMock.Setup(x => x.GetByIdAsync(userId))
                        .ReturnsAsync((AppUser)null);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _userService.GetCurrentUserByIdAsync(userId)
            );
        }

        [Fact]
        // Перевіряє отримання списку всіх користувачів
        public async Task GetAllUsersAsync_ShouldReturnUsersList()
        {
            // Підготовка
            var users = new List<AppUser>
            {
                new AppUser { Id = "1", Email = "user1@test.com" },
                new AppUser { Id = "2", Email = "user2@test.com" }
            };
            _userRepoMock.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(users);

            // Виконання
            var result = await _userService.GetAllUsersAsync();

            // Перевірка
            result.Should().HaveCount(2);
        }

        [Fact]
        // Перевіряє оновлення даних користувача
        public async Task UpdateUserASync_ShouldCallRepositoryUpdate()
        {
            // Підготовка
            var userId = "user123";
            var user = new AppUser { Id = userId, Email = "updated@test.com" };

            // Виконання
            await _userService.UpdateUserASync(userId, user);

            // Перевірка
            _userRepoMock.Verify(x => x.UpdateAsync(userId, user), Times.Once);
        }
    }
}
