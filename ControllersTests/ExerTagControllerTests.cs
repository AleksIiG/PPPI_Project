using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Controllers;
using api.Dto.ExerTagsDto;
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
    public class ExerTagControllerTests
    {
        private readonly Mock<IExerTagService> _exerTagServiceMock;
        private readonly ExerTagController _controller;

        public ExerTagControllerTests()
        {
            // Створюємо mock-об'єкт для сервісу тегів
            _exerTagServiceMock = new Mock<IExerTagService>();
            _controller = new ExerTagController(_exerTagServiceMock.Object);
        }

        [Fact]
        // Перевіряє отримання всіх тегів з фільтрацією
        public async Task GetAllAsync_ShouldReturnOk_WithTags()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            var tags = new List<ExerciseTag>
            {
                new ExerciseTag { Id = "1", Name = "Tag 1" },
                new ExerciseTag { Id = "2", Name = "Tag 2" }
            };

            _exerTagServiceMock
                .Setup(x => x.GetAllAsync(query))
                .ReturnsAsync(tags);

            // Виконання
            var result = await _controller.GetAllAsync(query);

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        // Перевіряє помилку при відсутності тегів
        public async Task GetAllAsync_ShouldReturnNotFound_WhenTagsAreNull()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            _exerTagServiceMock
                .Setup(x => x.GetAllAsync(query))
                .ReturnsAsync((List<ExerciseTag>)null);

            // Виконання
            var result = await _controller.GetAllAsync(query);

            // Перевірка
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        // Перевіряє помилку при невалідних даних запиту
        public async Task GetAllAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            _controller.ModelState.AddModelError("Name", "Error");

            // Виконання
            var result = await _controller.GetAllAsync(query);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє отримання тегу за існуючим ID
        public async Task GetById_ShouldReturnOk_WhenTagExists()
        {
            // Підготовка
            var tagId = "1";
            var tag = new ExerciseTag { Id = tagId, Name = "Tag 1" };

            _exerTagServiceMock
                .Setup(x => x.GetByIdAsync(tagId))
                .ReturnsAsync(tag);

            // Виконання
            var result = await _controller.GetById(tagId);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при отриманні неіснуючого тегу
        public async Task GetById_ShouldReturnNotFound_WhenTagDoesNotExist()
        {
            // Підготовка
            var tagId = "999";
            _exerTagServiceMock
                .Setup(x => x.GetByIdAsync(tagId))
                .ThrowsAsync(new KeyNotFoundException("Tag not found"));

            // Виконання
            var result = await _controller.GetById(tagId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Tag not found" });
        }

        [Fact]
        // Перевіряє помилку при невалідних даних запиту тегу
        public async Task GetById_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var tagId = "1";
            _controller.ModelState.AddModelError("Name", "Error");

            // Виконання
            var result = await _controller.GetById(tagId);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє успішне створення нового тегу
        public async Task CreateAsync_ShouldReturnCreated_WhenTagIsValid()
        {
            // Підготовка
            var createDto = new CreateExerTagDto { Name = "New Tag" };
            var createdTag = new ExerciseTag { Id = "1", Name = "New Tag" };

            _exerTagServiceMock
                .Setup(x => x.CreateAsync(It.IsAny<ExerciseTag>()))
                .ReturnsAsync(createdTag);

            // Виконання
            var result = await _controller.CreateAsync(createDto);

            // Перевірка
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        // Перевіряє помилку при створенні тегу з невалідними даними
        public async Task CreateAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var createDto = new CreateExerTagDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Виконання
            var result = await _controller.CreateAsync(createDto);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє конфлікт при спробі створити тег, який вже існує
        public async Task CreateAsync_ShouldReturnConflict_WhenInvalidOperationExceptionOccurs()
        {
            // Підготовка
            var createDto = new CreateExerTagDto { Name = "Existing Tag" };

            _exerTagServiceMock
                .Setup(x => x.CreateAsync(It.IsAny<ExerciseTag>()))
                .ThrowsAsync(new InvalidOperationException("Tag already exists"));

            // Виконання
            var result = await _controller.CreateAsync(createDto);

            // Перевірка
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().BeEquivalentTo(new { message = "Tag already exists" });
        }

        [Fact]
        // Перевіряє успішне оновлення існуючого тегу
        public async Task UpdateAsync_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            // Підготовка
            var tagId = "1";
            var updateDto = new UpdateExerTagDto { Name = "Updated Tag" };
            var updatedTag = new ExerciseTag { Id = tagId, Name = "Updated Tag" };

            _exerTagServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<ExerciseTag>(), tagId))
                .ReturnsAsync(updatedTag);

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при оновленні тегу з невалідними даними
        public async Task UpdateAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var tagId = "1";
            var updateDto = new UpdateExerTagDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при оновленні неіснуючого тегу
        public async Task UpdateAsync_ShouldReturnNotFound_WhenTagDoesNotExist()
        {
            // Підготовка
            var tagId = "999";
            var updateDto = new UpdateExerTagDto { Name = "Updated Tag" };

            _exerTagServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<ExerciseTag>(), tagId))
                .ThrowsAsync(new KeyNotFoundException("Tag not found"));

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Tag not found" });
        }

        [Fact]
        // Перевіряє конфлікт при оновленні тегу
        public async Task UpdateAsync_ShouldReturnConflict_WhenInvalidOperationExceptionOccurs()
        {
            // Підготовка
            var tagId = "1";
            var updateDto = new UpdateExerTagDto { Name = "Tag With Conflict" };

            _exerTagServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<ExerciseTag>(), tagId))
                .ThrowsAsync(new InvalidOperationException("Update conflict"));

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().BeEquivalentTo(new { message = "Update conflict" });
        }

        [Fact]
        // Перевіряє помилку при оновленні тегу, який повертає null
        public async Task UpdateAsync_ShouldReturnNotFound_WhenUpdatedTagIsNull()
        {
            // Підготовка
            var tagId = "999";
            var updateDto = new UpdateExerTagDto { Name = "Non-existent Tag" };

            _exerTagServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<ExerciseTag>(), tagId))
                .ReturnsAsync((ExerciseTag)null);

            // Виконання
            var result = await _controller.UpdateAsync(updateDto, tagId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = $"Tag with Id: {tagId} is not found." });
        }

        [Fact]
        // Перевіряє успішне видалення тегу
        public async Task DeleteAsync_ShouldReturnNoContent_WhenTagExists()
        {
            // Підготовка
            var tagId = "1";
            var deletedTag = new ExerciseTag { Id = tagId, Name = "Deleted Tag" };

            _exerTagServiceMock
                .Setup(x => x.DeleteAsync(tagId))
                .ReturnsAsync(deletedTag);

            // Виконання
            var result = await _controller.DeleteAsync(tagId);

            // Перевірка
            result.Should().BeOfType<NoContentResult>();
            _exerTagServiceMock.Verify(x => x.DeleteAsync(tagId), Times.Once);
        }

        [Fact]
        // Перевіряє помилку при видаленні неіснуючого тегу
        public async Task DeleteAsync_ShouldReturnNotFound_WhenTagDoesNotExist()
        {
            // Підготовка
            var tagId = "999";
            _exerTagServiceMock
                .Setup(x => x.DeleteAsync(tagId))
                .ThrowsAsync(new KeyNotFoundException("Tag not found"));

            // Виконання
            var result = await _controller.DeleteAsync(tagId);

            // Перевірка
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Tag not found" });
        }

        [Fact]
        // Перевіряє отримання пустого списку тегів
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoTagsExist()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            var tags = new List<ExerciseTag>();

            _exerTagServiceMock
                .Setup(x => x.GetAllAsync(query))
                .ReturnsAsync(tags);

            // Виконання
            var result = await _controller.GetAllAsync(query);

            // Перевірка
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }
    }
}