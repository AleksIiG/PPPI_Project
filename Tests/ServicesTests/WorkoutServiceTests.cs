using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Dto.ExerTagsDto;
using api.Helpers;
using api.Interface;
using api.Models;
using api.Services;
using api.Services.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;

namespace Tests.Services
{
    public class WorkoutServiceTests
    {
        private readonly Mock<IWorkoutRepository> _workoutRepoMock;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IWorkoutTagService> _workoutTagServiceMock;
        private readonly WorkoutService _workoutService;

        public WorkoutServiceTests()
        {
            // Створюємо mock-об'єкти для залежностей
            _workoutRepoMock = new Mock<IWorkoutRepository>();
            _exerciseServiceMock = new Mock<IExerciseService>();
            _workoutTagServiceMock = new Mock<IWorkoutTagService>();
            _workoutService = new WorkoutService(_workoutRepoMock.Object, _exerciseServiceMock.Object, _workoutTagServiceMock.Object);
        }

        [Fact]
        // Перевіряє успішне створення тренування
        public async Task CreateAsync_ShouldReturnWorkout_WhenAllDataIsValid()
        {
            // Підготовка
            var workout = new Workout
            {
                Name = "New Workout",
                TagsIds = new List<string> { "tag1" },
                ExerciseIds = new List<string> { "exercise1" }
            };
            var createdWorkout = new Workout { Id = "1", Name = "New Workout" };

            _workoutTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(workout.TagsIds))
                                .ReturnsAsync(new List<string>());
            _exerciseServiceMock.Setup(x => x.GetByIdAsync("exercise1"))
                              .ReturnsAsync(new Exercise());
            _workoutRepoMock.Setup(x => x.ExistsByNameAsync(workout.Name))
                          .ReturnsAsync(false);
            _workoutRepoMock.Setup(x => x.CreateAsync(workout))
                          .ReturnsAsync(createdWorkout);

            // Виконання
            var result = await _workoutService.CreateAsync(workout);

            // Перевірка
            result.Should().Be(createdWorkout);
        }

        [Fact]
        // Перевіряє помилку при спробі створити тренування з неіснуючими тегами
        public async Task CreateAsync_ShouldThrowException_WhenTagsDoNotExist()
        {
            // Підготовка
            var workout = new Workout { TagsIds = new List<string> { "nonexistent" } };
            _workoutTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(workout.TagsIds))
                                .ReturnsAsync(new List<string> { "nonexistent" });

            // Виконання & Перевірка
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _workoutService.CreateAsync(workout)
            );
        }

        [Fact]
        // Перевіряє помилку при спробі створити тренування з неіснуючими вправами
        public async Task CreateAsync_ShouldThrowException_WhenExerciseDoesNotExist()
        {
            // Підготовка
            var workout = new Workout { ExerciseIds = new List<string> { "nonexistent" } };
            _workoutTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(It.IsAny<List<string>>()))
                                .ReturnsAsync(new List<string>());
            _exerciseServiceMock.Setup(x => x.GetByIdAsync("nonexistent"))
                              .ThrowsAsync(new KeyNotFoundException());

            // Виконання & Перевірка
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _workoutService.CreateAsync(workout)
            );
        }

        [Fact]
        // Перевіряє помилку при спробі створити тренування з існуючою назвою
        public async Task CreateAsync_ShouldThrowException_WhenNameAlreadyExists()
        {
            // Підготовка
            var workout = new Workout { Name = "Existing Workout" };
            _workoutTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(It.IsAny<List<string>>()))
                                .ReturnsAsync(new List<string>());
            _workoutRepoMock.Setup(x => x.ExistsByNameAsync(workout.Name))
                          .ReturnsAsync(true);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _workoutService.CreateAsync(workout)
            );
        }

        [Fact]
        // Перевіряє успішне видалення тренування
        public async Task DeleteAsync_ShouldReturnWorkout_WhenWorkoutExists()
        {
            // Підготовка
            var workoutId = "1";
            var workout = new Workout { Id = workoutId, Name = "Test Workout" };
            _workoutRepoMock.Setup(x => x.GetByIdAsync(workoutId))
                          .ReturnsAsync(workout);
            _workoutRepoMock.Setup(x => x.DeleteAsync(workoutId))
                          .ReturnsAsync(workout);

            // Виконання
            var result = await _workoutService.DeleteAsync(workoutId);

            // Перевірка
            result.Should().Be(workout);
        }

        [Fact]
        // Перевіряє помилку при спробі видалити неіснуюче тренування
        public async Task DeleteAsync_ShouldThrowException_WhenWorkoutNotFound()
        {
            // Підготовка
            var workoutId = "999";
            _workoutRepoMock.Setup(x => x.GetByIdAsync(workoutId))
                          .ReturnsAsync((Workout)null);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _workoutService.DeleteAsync(workoutId)
            );
        }

        [Fact]
        // Перевіряє отримання тренування за існуючим ID
        public async Task GetByIdAsync_ShouldReturnWorkout_WhenWorkoutExists()
        {
            // Підготовка
            var workoutId = "1";
            var workout = new Workout { Id = workoutId, Name = "Test Workout" };
            _workoutRepoMock.Setup(x => x.GetByIdAsync(workoutId))
                          .ReturnsAsync(workout);

            // Виконання
            var result = await _workoutService.GetByIdAsync(workoutId);

            // Перевірка
            result.Should().Be(workout);
        }

        [Fact]
        // Перевіряє помилку при отриманні неіснуючого тренування
        public async Task GetByIdAsync_ShouldThrowException_WhenWorkoutNotFound()
        {
            // Підготовка
            var workoutId = "999";
            _workoutRepoMock.Setup(x => x.GetByIdAsync(workoutId))
                          .ReturnsAsync((Workout)null);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _workoutService.GetByIdAsync(workoutId)
            );
        }

        [Fact]
        // Перевіряє успішне оновлення тренування
        public async Task UpdateAsync_ShouldReturnWorkout_WhenAllDataIsValid()
        {
            // Підготовка
            var workoutId = "1";
            var existingWorkout = new Workout { Id = workoutId, Name = "Old Name" };
            var updatedWorkout = new Workout
            {
                Id = workoutId,
                Name = "New Name",
                TagsIds = new List<string> { "tag1" },
                ExerciseIds = new List<string> { "exercise1" }
            };

            _workoutRepoMock.Setup(x => x.GetByIdAsync(workoutId))
                          .ReturnsAsync(existingWorkout);
            _workoutRepoMock.Setup(x => x.ExistsByNameAsync(updatedWorkout.Name))
                          .ReturnsAsync(false);
            _workoutTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(updatedWorkout.TagsIds))
                                .ReturnsAsync(new List<string>());
            _exerciseServiceMock.Setup(x => x.GetByIdAsync("exercise1"))
                              .ReturnsAsync(new Exercise());

            // Виконання
            var result = await _workoutService.UpdateAsync(workoutId, updatedWorkout);

            // Перевірка
            result.Should().Be(updatedWorkout);
        }
    }
}
