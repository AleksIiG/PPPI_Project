using api.Controllers;
using api.Dto.ExerciseDTOs;
using api.Dto.ExerTagsDto;
using api.Helpers;
using api.Interface;
using api.Models;
using api.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controllers
{
    public class ExerciseControllerTests
    {
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IExerTagService> _exerTagServiceMock;
        private readonly ExerciseController _controller;

        public ExerciseControllerTests()
        {
            // Створюємо mock-об'єкти для сервісів
            _exerciseServiceMock = new Mock<IExerciseService>();
            _exerTagServiceMock = new Mock<IExerTagService>();
            _controller = new ExerciseController(_exerciseServiceMock.Object, _exerTagServiceMock.Object);
        }

        [Fact]
        // Перевіряє отримання всіх вправ з фільтрацією
        public async Task GetAll_ShouldReturnOk_WithExercises()
        {
            // Підготовка
            var query = new QueryObjectForExercises();
            var exercises = new List<Exercise>
            {
                new Exercise { Id = "1", Name = "Exercise 1", TagsIds = new List<string> { "tag1" } }
            };

            _exerciseServiceMock
                .Setup(x => x.GetAllAsync(query))
                .ReturnsAsync(exercises);

            _exerTagServiceMock
                .Setup(x => x.GetByIdsFromExercisesAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<ExerTagDto>());

            // Виконання
            var result = await _controller.GetAll(query);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє отримання вправи за існуючим ID
        public async Task GetById_ShouldReturnOk_WhenExerciseExists()
        {
            // Підготовка
            var exerciseId = "1";
            var exercise = new Exercise { Id = exerciseId, Name = "Exercise 1", TagsIds = new List<string> { "tag1" } };

            _exerciseServiceMock
                .Setup(x => x.GetByIdAsync(exerciseId))
                .ReturnsAsync(exercise);

            _exerTagServiceMock
                .Setup(x => x.GetByIdsFromExercisesAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<ExerTagDto>());

            // Виконання
            var result = await _controller.GetById(exerciseId);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при отриманні неіснуючої вправи
        public async Task GetById_ShouldReturnNotFound_WhenExerciseDoesNotExist()
        {
            // Підготовка
            var exerciseId = "999";
            _exerciseServiceMock
                .Setup(x => x.GetByIdAsync(exerciseId))
                .ThrowsAsync(new KeyNotFoundException("Exercise not found"));

            // Виконання
            var result = await _controller.GetById(exerciseId);

            // Перевірка
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        // Перевіряє успішне видалення вправи
        public async Task Delete_ShouldReturnNoContent_WhenExerciseExists()
        {
            // Підготовка
            var exerciseId = "1";
            var deletedExercise = new Exercise { Id = exerciseId, Name = "Deleted Exercise" };

            _exerciseServiceMock
                .Setup(x => x.DeleteAsync(exerciseId))
                .ReturnsAsync(deletedExercise);

            // Виконання
            var result = await _controller.Delete(exerciseId);

            // Перевірка
            result.Should().BeOfType<NoContentResult>();
            _exerciseServiceMock.Verify(x => x.DeleteAsync(exerciseId), Times.Once);
        }

        [Fact]
        // Перевіряє помилку при видаленні неіснуючої вправи
        public async Task Delete_ShouldReturnNotFound_WhenExerciseDoesNotExist()
        {
            // Підготовка
            var exerciseId = "999";
            _exerciseServiceMock
                .Setup(x => x.DeleteAsync(exerciseId))
                .ThrowsAsync(new KeyNotFoundException("Exercise not found"));

            // Виконання
            var result = await _controller.Delete(exerciseId);

            // Перевірка
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        // Перевіряє успішне створення нової вправи
        public async Task Create_ShouldReturnCreated_WhenExerciseIsValid()
        {
            // Підготовка
            var createDto = new CreateExerciseDto
            {
                Name = "New Exercise",
                Description = "Description"
            };

            var createdExercise = new Exercise { Id = "1", Name = "New Exercise" };

            _exerciseServiceMock
                .Setup(x => x.CreateAsync(It.IsAny<Exercise>()))
                .ReturnsAsync(createdExercise);

            // Виконання
            var result = await _controller.Create(createDto);

            // Перевірка
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        // Перевіряє помилку при створенні вправи з невалідними даними
        public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var createDto = new CreateExerciseDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Виконання
            var result = await _controller.Create(createDto);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє конфлікт при спробі створити вправу, яка вже існує
        public async Task Create_ShouldReturnConflict_WhenInvalidOperationExceptionOccurs()
        {
            // Підготовка
            var createDto = new CreateExerciseDto { Name = "Existing Exercise" };

            _exerciseServiceMock
                .Setup(x => x.CreateAsync(It.IsAny<Exercise>()))
                .ThrowsAsync(new InvalidOperationException("Exercise already exists"));

            // Виконання
            var result = await _controller.Create(createDto);

            // Перевірка
            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        // Перевіряє успішне оновлення існуючої вправи
        public async Task Update_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            // Підготовка
            var exerciseId = "1";
            var updateDto = new UpdateExerciseDto { Name = "Updated Exercise" };
            var updatedExercise = new Exercise { Id = exerciseId, Name = "Updated Exercise" };

            _exerciseServiceMock
                .Setup(x => x.UpdateAsync(exerciseId, It.IsAny<Exercise>()))
                .ReturnsAsync(updatedExercise);

            // Виконання
            var result = await _controller.Update(updateDto, exerciseId);

            // Перевірка
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при оновленні вправи з невалідними даними
        public async Task Update_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Підготовка
            var exerciseId = "1";
            var updateDto = new UpdateExerciseDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Виконання
            var result = await _controller.Update(updateDto, exerciseId);

            // Перевірка
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        // Перевіряє помилку при оновленні неіснуючої вправи
        public async Task Update_ShouldReturnNotFound_WhenExerciseDoesNotExist()
        {
            // Підготовка
            var exerciseId = "999";
            var updateDto = new UpdateExerciseDto { Name = "Updated Exercise" };

            _exerciseServiceMock
                .Setup(x => x.UpdateAsync(exerciseId, It.IsAny<Exercise>()))
                .ThrowsAsync(new KeyNotFoundException("Exercise not found"));

            // Виконання
            var result = await _controller.Update(updateDto, exerciseId);

            // Перевірка
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        // Перевіряє конфлікт при оновленні вправи
        public async Task Update_ShouldReturnConflict_WhenInvalidOperationExceptionOccurs()
        {
            // Підготовка
            var exerciseId = "1";
            var updateDto = new UpdateExerciseDto { Name = "Exercise With Conflict" };

            _exerciseServiceMock
                .Setup(x => x.UpdateAsync(exerciseId, It.IsAny<Exercise>()))
                .ThrowsAsync(new InvalidOperationException("Update conflict"));

            // Виконання
            var result = await _controller.Update(updateDto, exerciseId);

            // Перевірка
            result.Should().BeOfType<ConflictObjectResult>();
        }
    }
}
