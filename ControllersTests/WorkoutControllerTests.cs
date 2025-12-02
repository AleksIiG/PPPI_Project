using api.Controllers;
using api.Dto.ExerciseDTOs;
using api.Dto.WorkoutDto;
using api.Dto.WorkoutTagDtos;
using api.Helpers;
using api.Interface;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controllers
{
    public class WorkoutControllerTests
    {
        private readonly Mock<IWorkoutService> _workoutServiceMock;
        private readonly Mock<IWorkoutTagService> _workoutTagServiceMock;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly WorkoutController _controller;

        public WorkoutControllerTests()
        {
            // Створюємо mock-об'єкти для сервісів тренувань
            _workoutServiceMock = new Mock<IWorkoutService>();
            _workoutTagServiceMock = new Mock<IWorkoutTagService>();
            _exerciseServiceMock = new Mock<IExerciseService>();
            _controller = new WorkoutController(
                _workoutServiceMock.Object,
                _workoutTagServiceMock.Object,
                _exerciseServiceMock.Object
            );
        }

        [Fact]
        // Перевіряє отримання всіх тренувань з фільтрацією
        public async Task GetAll_ReturnsOkResult()
        {
            // Підготовка
            var query = new QueryObjectForWorkouts();
            var workouts = new List<Workout> { new Workout { Id = "1", Name = "Test Workout" } };

            _workoutServiceMock.Setup(x => x.GetAllAsync(query)).ReturnsAsync(workouts);
            _workoutTagServiceMock.Setup(x => x.GetByIdsFromWorkoutsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkoutTagDto>());
            _exerciseServiceMock.Setup(x => x.GetByIdsFromWorkoutsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<ExerciseDto>());

            // Виконання
            var result = await _controller.GetAll(query);

            // Перевірка
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Перевіряє отримання тренування за існуючим ID
        public async Task GetById_ExistingWorkout_ReturnsOkResult()
        {
            // Підготовка
            var workoutId = "1";
            var workout = new Workout { Id = workoutId, Name = "Test Workout" };

            _workoutServiceMock.Setup(x => x.GetByIdAsync(workoutId)).ReturnsAsync(workout);
            _workoutTagServiceMock.Setup(x => x.GetByIdsFromWorkoutsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkoutTagDto>());
            _exerciseServiceMock.Setup(x => x.GetByIdsFromWorkoutsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<ExerciseDto>());

            // Виконання
            var result = await _controller.GetById(workoutId);

            // Перевірка
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Перевіряє помилку при отриманні неіснуючого тренування
        public async Task GetById_NonExistingWorkout_ReturnsNotFound()
        {
            // Підготовка
            var workoutId = "999";
            _workoutServiceMock.Setup(x => x.GetByIdAsync(workoutId)).ThrowsAsync(new KeyNotFoundException());

            // Виконання
            var result = await _controller.GetById(workoutId);

            // Перевірка
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Перевіряє помилку при невалідних даних запиту
        public async Task GetById_InvalidModel_ReturnsBadRequest()
        {
            // Підготовка
            var workoutId = "1";
            _controller.ModelState.AddModelError("Error", "Test error");

            // Виконання
            var result = await _controller.GetById(workoutId);

            // Перевірка
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // Перевіряє успішне створення нового тренування
        public async Task Create_ValidWorkout_ReturnsCreatedResult()
        {
            // Підготовка
            var createDto = new CreateWorkoutDto { Name = "New Workout" };
            var createdWorkout = new Workout { Id = "1", Name = "New Workout" };

            _workoutServiceMock.Setup(x => x.CreateAsync(It.IsAny<Workout>())).ReturnsAsync(createdWorkout);

            // Виконання
            var result = await _controller.Create(createDto);

            // Перевірка
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        // Перевіряє помилку при створенні тренування з невалідними даними
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            // Підготовка
            var createDto = new CreateWorkoutDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Виконання
            var result = await _controller.Create(createDto);

            // Перевірка
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // Перевіряє конфлікт при спробі створити тренування, яке вже існує
        public async Task Create_Conflict_ReturnsConflict()
        {
            // Підготовка
            var createDto = new CreateWorkoutDto { Name = "Existing Workout" };
            _workoutServiceMock.Setup(x => x.CreateAsync(It.IsAny<Workout>())).ThrowsAsync(new InvalidOperationException());

            // Виконання
            var result = await _controller.Create(createDto);

            // Перевірка
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        // Перевіряє успішне оновлення існуючого тренування
        public async Task Update_ValidWorkout_ReturnsOkResult()
        {
            // Підготовка
            var workoutId = "1";
            var updateDto = new UpdateWorkoutDto { Name = "Updated Workout" };
            var updatedWorkout = new Workout { Id = workoutId, Name = "Updated Workout" };

            _workoutServiceMock.Setup(x => x.UpdateAsync(workoutId, It.IsAny<Workout>())).ReturnsAsync(updatedWorkout);

            // Виконання
            var result = await _controller.Update(updateDto, workoutId);

            // Перевірка
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Перевіряє помилку при оновленні тренування з невалідними даними
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            // Підготовка
            var workoutId = "1";
            var updateDto = new UpdateWorkoutDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Виконання
            var result = await _controller.Update(updateDto, workoutId);

            // Перевірка
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // Перевіряє помилку при оновленні неіснуючого тренування
        public async Task Update_NonExistingWorkout_ReturnsNotFound()
        {
            // Підготовка
            var workoutId = "999";
            var updateDto = new UpdateWorkoutDto { Name = "Updated Workout" };
            _workoutServiceMock.Setup(x => x.UpdateAsync(workoutId, It.IsAny<Workout>())).ThrowsAsync(new KeyNotFoundException());

            // Виконання
            var result = await _controller.Update(updateDto, workoutId);

            // Перевірка
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Перевіряє успішне видалення тренування
        public async Task Delete_ExistingWorkout_ReturnsNoContent()
        {
            // Підготовка
            var workoutId = "1";
            var deletedWorkout = new Workout { Id = workoutId, Name = "Deleted Workout" };
            _workoutServiceMock.Setup(x => x.DeleteAsync(workoutId)).ReturnsAsync(deletedWorkout);

            // Виконання
            var result = await _controller.Delete(workoutId);

            // Перевірка
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        // Перевіряє помилку при видаленні неіснуючого тренування
        public async Task Delete_NonExistingWorkout_ReturnsNotFound()
        {
            // Підготовка
            var workoutId = "999";
            _workoutServiceMock.Setup(x => x.DeleteAsync(workoutId)).ThrowsAsync(new KeyNotFoundException());

            // Виконання
            var result = await _controller.Delete(workoutId);

            // Перевірка
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}