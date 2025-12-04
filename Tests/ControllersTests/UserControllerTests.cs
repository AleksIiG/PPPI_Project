using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Controllers;
using api.Dto.UserDTOs;
using api.Interface;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUSerService> _userServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            // Створюємо mock-об'єкт для сервісу користувачів
            _userServiceMock = new Mock<IUSerService>();
            _controller = new UserController(_userServiceMock.Object);
        }

        [Fact]
        // Перевіряє отримання поточного користувача при автентифікації
        public async Task GetCurrentUser_ShouldReturnOk_WhenUserExistsAndIsAuthenticated()
        {
            // Підготовка
            var userId = "user123";
            var user = new AppUser { Id = userId, Email = "test@test.com" };

            // Налаштовуємо HTTP контекст з claims користувача
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _userServiceMock
                .Setup(x => x.GetCurrentUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Виконання
            var result = await _controller.GetCurrentUser();

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє помилку при відсутності ID користувача в токені
        public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenUserIdNotFoundInToken()
        {
            // Підготовка
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Виконання
            var result = await _controller.GetCurrentUser();

            // Перевірка
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "User ID not found in token." });
        }

        [Fact]
        // Перевіряє помилку при отриманні неіснуючого користувача
        public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Підготовка
            var userId = "nonexistent";

            // Налаштовуємо HTTP контекст з claims користувача
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _userServiceMock
                .Setup(x => x.GetCurrentUserByIdAsync(userId))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            // Виконання
            var result = await _controller.GetCurrentUser();

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "User not found" });
        }

        [Fact]
        // Перевіряє обробку внутрішньої помилки сервера
        public async Task GetCurrentUser_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Підготовка
            var userId = "user123";

            // Налаштовуємо HTTP контекст з claims користувача
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _userServiceMock
                .Setup(x => x.GetCurrentUserByIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Виконання
            var result = await _controller.GetCurrentUser();

            // Перевірка
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Database error" });
        }

        [Fact]
        // Перевіряє отримання списку всіх користувачів
        public async Task GetAllUsers_ShouldReturnOk_WithUsersList()
        {
            // Підготовка
            var users = new List<AppUser>
            {
                new AppUser { Id = "1", Email = "user1@test.com" },
                new AppUser { Id = "2", Email = "user2@test.com" }
            };

            _userServiceMock
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Виконання
            var result = await _controller.GetAllUsers();

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var userDtos = okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject;
            userDtos.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє помилку при отриманні списку користувачів
        public async Task GetAllUsers_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Підготовка
            _userServiceMock
                .Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Виконання
            var result = await _controller.GetAllUsers();

            // Перевірка
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Database error" });
        }

        [Fact]
        // Перевіряє успішне підвищення прав користувача до адміністратора
        public async Task PromoteToAdmin_ShouldReturnOk_WhenPromotionSuccessful()
        {
            // Підготовка
            var userId = "user123";
            var user = new AppUser { Id = userId, Email = "test@test.com", Role = "User" };

            _userServiceMock
                .Setup(x => x.GetCurrentUserByIdAsync(userId))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.UpdateUserASync(userId, It.IsAny<AppUser>()))
                .Returns(Task.CompletedTask);

            // Виконання
            var result = await _controller.PromoteToAdmin(userId);

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { message = $"User with id {userId} promoted to Admin." });

            _userServiceMock.Verify(x => x.UpdateUserASync(userId, It.Is<AppUser>(u => u.Role == "Admin")), Times.Once);
        }

        [Fact]
        // Перевіряє помилку при спробі підвищити неіснуючого користувача
        public async Task PromoteToAdmin_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Підготовка
            var userId = "nonexistent";

            _userServiceMock
                .Setup(x => x.GetCurrentUserByIdAsync(userId))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            // Виконання
            var result = await _controller.PromoteToAdmin(userId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "User not found" });
        }

        [Fact]
        // Перевіряє помилку при спробі підвищити користувача, який повертає null
        public async Task PromoteToAdmin_ShouldReturnNotFound_WhenUserIsNull()
        {
            // Підготовка
            var userId = "nonexistent";

            _userServiceMock
                .Setup(x => x.GetCurrentUserByIdAsync(userId))
                .ReturnsAsync((AppUser)null);

            // Виконання
            var result = await _controller.PromoteToAdmin(userId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = $"User with id {userId} not found." });
        }

        [Fact]
        // Перевіряє помилку при оновленні ролі користувача
        public async Task PromoteToAdmin_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Підготовка
            var userId = "user123";
            var user = new AppUser { Id = userId, Email = "test@test.com" };

            _userServiceMock
                .Setup(x => x.GetCurrentUserByIdAsync(userId))
                .ReturnsAsync(user);

            _userServiceMock
                .Setup(x => x.UpdateUserASync(userId, It.IsAny<AppUser>()))
                .ThrowsAsync(new Exception("Update failed"));

            // Виконання
            var result = await _controller.PromoteToAdmin(userId);

            // Перевірка
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Update failed" });
        }

        [Fact]
        // Перевіряє отримання пустого списку користувачів
        public async Task GetAllUsers_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Підготовка
            var users = new List<AppUser>();

            _userServiceMock
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Виконання
            var result = await _controller.GetAllUsers();

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var userDtos = okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject;
            userDtos.Should().BeEmpty();
        }

        [Fact]
        // Перевіряє коректність оновлення ролі користувача на адміністратора
        public async Task PromoteToAdmin_ShouldUpdateRoleToAdmin()
        {
            // Підготовка
            var userId = "user123";
            var originalUser = new AppUser { Id = userId, Email = "test@test.com", Role = "User" };
            AppUser updatedUser = null;

            _userServiceMock
                .Setup(x => x.GetCurrentUserByIdAsync(userId))
                .ReturnsAsync(originalUser);

            _userServiceMock
                .Setup(x => x.UpdateUserASync(userId, It.IsAny<AppUser>()))
                .Callback<string, AppUser>((id, user) => updatedUser = user)
                .Returns(Task.CompletedTask);

            // Виконання
            var result = await _controller.PromoteToAdmin(userId);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
            updatedUser.Should().NotBeNull();
            updatedUser.Role.Should().Be("Admin");
            updatedUser.Id.Should().Be(userId);
            updatedUser.Email.Should().Be("test@test.com");
        }
    }
}
