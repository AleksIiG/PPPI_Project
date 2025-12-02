using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Controllers;
using api.Dto.WorkoutTagDtos;
using api.Helpers;
using api.Interface;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace Tests.Controllers
{
    public class WorkoutTagControllerTests
    {
        private readonly Mock<IWorkoutTagService> _workoutTagServiceMock;
        private readonly WorkoutTagController _controller;

        public WorkoutTagControllerTests()
        {
            // Створюємо mock-об'єкт для сервісу тегів тренувань
            _workoutTagServiceMock = new Mock<IWorkoutTagService>();
            _controller = new WorkoutTagController(_workoutTagServiceMock.Object);
        }

        [Fact]
        // Перевіряє отримання всіх тегів тренувань з фільтрацією
        public async Task GetAllAsync_ReturnsOkResult_WithTags()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            var tags = new List<WorkoutTag>
            {
                new WorkoutTag { Id = "1", Name = "Tag 1" },
                new WorkoutTag { Id = "2", Name = "Tag 2" }
            };

            _workoutTagServiceMock.Setup(x => x.GetAllAsync(query)).ReturnsAsync(tags);

            // Виконання
            var result = await _controller.GetAllAsync(query);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при відсутності тегів тренувань
        public async Task GetAllAsync_ReturnsNotFound_WhenTagsAreNull()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            _workoutTagServiceMock.Setup(x => x.GetAllAsync(query)).ReturnsAsync((List<WorkoutTag>)null);

            // Виконання
            var result = await _controller.GetAllAsync(query);

            // Перевірка
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        // Перевіряє помилку при невалідних даних запиту
        public async Task GetAllAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            _controller.ModelState.AddModelError("Error", "Test error");

            // Виконання
            var result = await _controller.GetAllAsync(query);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє отримання тегу тренування за існуючим ID
        public async Task GetById_ReturnsOkResult_WhenTagExists()
        {
            // Підготовка
            var tagId = "1";
            var tag = new WorkoutTag { Id = tagId, Name = "Tag 1" };

            _workoutTagServiceMock.Setup(x => x.GetByIdAsync(tagId)).ReturnsAsync(tag);

            // Виконання
            var result = await _controller.GetById(tagId);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при отриманні неіснуючого тегу тренування
        public async Task GetById_ReturnsNotFound_WhenTagDoesNotExist()
        {
            // Підготовка
            var tagId = "999";
            _workoutTagServiceMock.Setup(x => x.GetByIdAsync(tagId)).ThrowsAsync(new KeyNotFoundException("Tag not found"));

            // Виконання
            var result = await _controller.GetById(tagId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє помилку при невалідних даних запиту тегу
        public async Task GetById_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var tagId = "1";
            _controller.ModelState.AddModelError("Error", "Test error");

            // Виконання
            var result = await _controller.GetById(tagId);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє успішне створення нового тегу тренування
        public async Task CreateAsync_ReturnsCreatedResult_WhenTagIsValid()
        {
            // Підготовка
            var createDto = new CreateWorkoutTagDto { Name = "New Tag" };
            var createdTag = new WorkoutTag { Id = "1", Name = "New Tag" };

            _workoutTagServiceMock.Setup(x => x.CreateAsync(It.IsAny<WorkoutTag>())).ReturnsAsync(createdTag);

            // Виконання
            var result = await _controller.CreateAsync(createDto);

            // Перевірка
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(_controller.GetById));
            createdResult.RouteValues["id"].Should().Be("1");
        }

        [Fact]
        // Перевіряє помилку при створенні тегу з невалідними даними
        public async Task CreateAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var createDto = new CreateWorkoutTagDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Виконання
            var result = await _controller.CreateAsync(createDto);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє конфлікт при спробі створити тег, який вже існує
        public async Task CreateAsync_ReturnsConflict_WhenInvalidOperationExceptionOccurs()
        {
            // Підготовка
            var createDto = new CreateWorkoutTagDto { Name = "Existing Tag" };
            _workoutTagServiceMock.Setup(x => x.CreateAsync(It.IsAny<WorkoutTag>())).ThrowsAsync(new InvalidOperationException("Tag already exists"));

            // Виконання
            var result = await _controller.CreateAsync(createDto);

            // Перевірка
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє успішне оновлення існуючого тегу тренування
        public async Task UpdateAsync_ReturnsOkResult_WhenUpdateIsSuccessful()
        {
            // Підготовка
            var tagId = "1";
            var updateDto = new UpdateWorkoutTagDto { Name = "Updated Tag" };
            var updatedTag = new WorkoutTag { Id = tagId, Name = "Updated Tag" };

            _workoutTagServiceMock.Setup(x => x.UpdateAsync(It.IsAny<WorkoutTag>(), tagId)).ReturnsAsync(updatedTag);

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(updatedTag);
        }

        [Fact]
        // Перевіряє помилку при оновленні тегу з невалідними даними
        public async Task UpdateAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var tagId = "1";
            var updateDto = new UpdateWorkoutTagDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при оновленні неіснуючого тегу тренування
        public async Task UpdateAsync_ReturnsNotFound_WhenTagDoesNotExist()
        {
            // Підготовка
            var tagId = "999";
            var updateDto = new UpdateWorkoutTagDto { Name = "Updated Tag" };
            _workoutTagServiceMock.Setup(x => x.UpdateAsync(It.IsAny<WorkoutTag>(), tagId)).ThrowsAsync(new KeyNotFoundException("Tag not found"));

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє конфлікт при оновленні тегу тренування
        public async Task UpdateAsync_ReturnsConflict_WhenInvalidOperationExceptionOccurs()
        {
            // Підготовка
            var tagId = "1";
            var updateDto = new UpdateWorkoutTagDto { Name = "Tag With Conflict" };
            _workoutTagServiceMock.Setup(x => x.UpdateAsync(It.IsAny<WorkoutTag>(), tagId)).ThrowsAsync(new InvalidOperationException("Update conflict"));

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє помилку при оновленні тегу, який повертає null
        public async Task UpdateAsync_ReturnsNotFound_WhenUpdatedTagIsNull()
        {
            // Підготовка
            var tagId = "999";
            var updateDto = new UpdateWorkoutTagDto { Name = "Non-existent Tag" };
            _workoutTagServiceMock.Setup(x => x.UpdateAsync(It.IsAny<WorkoutTag>(), tagId)).ReturnsAsync((WorkoutTag)null);

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє успішне видалення тегу тренування
        public async Task DeleteAsync_ReturnsNoContent_WhenTagExists()
        {
            // Підготовка
            var tagId = "1";
            var deletedTag = new WorkoutTag { Id = tagId, Name = "Deleted Tag" };
            _workoutTagServiceMock.Setup(x => x.DeleteAsync(tagId)).ReturnsAsync(deletedTag);

            // Виконання
            var result = await _controller.DeleteAsync(tagId);

            // Перевірка
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        // Перевіряє помилку при видаленні неіснуючого тегу тренування
        public async Task DeleteAsync_ReturnsNotFound_WhenTagDoesNotExist()
        {
            // Підготовка
            var tagId = "999";
            _workoutTagServiceMock.Setup(x => x.DeleteAsync(tagId)).ThrowsAsync(new KeyNotFoundException("Tag not found"));

            // Виконання
            var result = await _controller.DeleteAsync(tagId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє отримання пустого списку тегів тренувань
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoTagsExist()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            var tags = new List<WorkoutTag>();

            _workoutTagServiceMock.Setup(x => x.GetAllAsync(query)).ReturnsAsync(tags);

            // Виконання
            var result = await _controller.GetAllAsync(query);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}