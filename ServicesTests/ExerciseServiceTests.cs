using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerciseDTOs;
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
    public class ExerciseServiceTests
    {
        private readonly Mock<IExerciseRepository> _exerciseRepoMock;
        private readonly Mock<IExerTagService> _exerTagServiceMock;
        private readonly ExerciseService _exerciseService;

        public ExerciseServiceTests()
        {
            // Створюємо mock-об'єкти для репозиторію вправ та сервісу тегів
            _exerciseRepoMock = new Mock<IExerciseRepository>();
            _exerTagServiceMock = new Mock<IExerTagService>();
            _exerciseService = new ExerciseService(_exerciseRepoMock.Object, _exerTagServiceMock.Object);
        }

        [Fact]
        // Перевіряє успішне створення нової вправи
        public async Task CreateAsync_ValidExercise_ReturnsExercise()
        {
            // Підготовка
            var exercise = new Exercise { Name = "New Exercise", TagsIds = new List<string> { "tag1" } };
            var createdExercise = new Exercise { Id = "1", Name = "New Exercise" };

            _exerTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(exercise.TagsIds)).ReturnsAsync(new List<string>());
            _exerciseRepoMock.Setup(x => x.ExistsByNameAsync(exercise.Name)).ReturnsAsync(false);
            _exerciseRepoMock.Setup(x => x.CreateAsync(exercise)).ReturnsAsync(createdExercise);

            // Виконання
            var result = await _exerciseService.CreateAsync(exercise);

            // Перевірка
            result.Should().Be(createdExercise);
        }

        [Fact]
        // Перевіряє помилку при спробі створити вправу з неіснуючими тегами
        public async Task CreateAsync_NonExistingTags_ThrowsException()
        {
            // Підготовка
            var exercise = new Exercise { Name = "New Exercise", TagsIds = new List<string> { "nonexistent" } };
            _exerTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(exercise.TagsIds))
                .ReturnsAsync(new List<string> { "nonexistent" });

            // Виконання & Перевірка
            await Assert.ThrowsAsync<InvalidOperationException>(() => _exerciseService.CreateAsync(exercise));
        }

        [Fact]
        // Перевіряє помилку при спробі створити вправу з існуючою назвою
        public async Task CreateAsync_ExistingName_ThrowsException()
        {
            // Підготовка
            var exercise = new Exercise { Name = "Existing Exercise", TagsIds = new List<string>() };
            _exerTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(exercise.TagsIds)).ReturnsAsync(new List<string>());
            _exerciseRepoMock.Setup(x => x.ExistsByNameAsync(exercise.Name)).ReturnsAsync(true);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<InvalidOperationException>(() => _exerciseService.CreateAsync(exercise));
        }

        [Fact]
        // Перевіряє успішне видалення існуючої вправи
        public async Task DeleteAsync_ExistingExercise_ReturnsExercise()
        {
            // Підготовка
            var exerciseId = "1";
            var exercise = new Exercise { Id = exerciseId, Name = "Exercise 1" };
            _exerciseRepoMock.Setup(x => x.GetByIdAsync(exerciseId)).ReturnsAsync(exercise);
            _exerciseRepoMock.Setup(x => x.DeleteAsync(exerciseId)).ReturnsAsync(exercise);

            // Виконання
            var result = await _exerciseService.DeleteAsync(exerciseId);

            // Перевірка
            result.Should().Be(exercise);
        }

        [Fact]
        // Перевіряє помилку при спробі видалити неіснуючу вправу
        public async Task DeleteAsync_NonExistingExercise_ThrowsException()
        {
            // Підготовка
            var exerciseId = "999";
            _exerciseRepoMock.Setup(x => x.GetByIdAsync(exerciseId)).ReturnsAsync((Exercise)null);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _exerciseService.DeleteAsync(exerciseId));
        }

        [Fact]
        // Перевіряє отримання вправ за ID з тренувань разом з тегами
        public async Task GetByIdsFromWorkoutsAsync_ReturnsExercisesWithTags()
        {
            // Підготовка
            var exerciseIds = new List<string> { "1", "2" };
            var exercises = new List<Exercise>
            {
                new Exercise { Id = "1", Name = "Exercise 1", TagsIds = new List<string> { "tag1" } },
                new Exercise { Id = "2", Name = "Exercise 2", TagsIds = new List<string> { "tag2" } }
            };

            foreach (var exercise in exercises)
            {
                _exerciseRepoMock.Setup(x => x.GetByIdAsync(exercise.Id)).ReturnsAsync(exercise);
            }

            var allTagIds = exercises.SelectMany(e => e.TagsIds).Distinct().ToList();
            var tags = allTagIds.Select(id => new ExerTagDto { Id = id, Name = $"Tag {id}" }).ToList();
            _exerTagServiceMock.Setup(x => x.GetByIdsFromExercisesAsync(It.IsAny<List<string>>())).ReturnsAsync(tags);

            // Виконання
            var result = await _exerciseService.GetByIdsFromWorkoutsAsync(exerciseIds);

            // Перевірка
            result.Should().HaveCount(2);
        }

        [Fact]
        // Перевіряє отримання всіх вправ з фільтрацією
        public async Task GetAllAsync_ReturnsExercises()
        {
            // Підготовка
            var query = new QueryObjectForExercises();
            var exercises = new List<Exercise> { new Exercise { Id = "1" }, new Exercise { Id = "2" } };
            _exerciseRepoMock.Setup(x => x.GetAllAsync(query)).ReturnsAsync(exercises);

            // Виконання
            var result = await _exerciseService.GetAllAsync(query);

            // Перевірка
            result.Should().HaveCount(2);
        }

        [Fact]
        // Перевіряє отримання вправи за існуючим ID
        public async Task GetByIdAsync_ExistingExercise_ReturnsExercise()
        {
            // Підготовка
            var exerciseId = "1";
            var exercise = new Exercise { Id = exerciseId, Name = "Exercise 1" };
            _exerciseRepoMock.Setup(x => x.GetByIdAsync(exerciseId)).ReturnsAsync(exercise);

            // Виконання
            var result = await _exerciseService.GetByIdAsync(exerciseId);

            // Перевірка
            result.Should().Be(exercise);
        }

        [Fact]
        // Перевіряє помилку при отриманні неіснуючої вправи
        public async Task GetByIdAsync_NonExistingExercise_ThrowsException()
        {
            // Підготовка
            var exerciseId = "999";
            _exerciseRepoMock.Setup(x => x.GetByIdAsync(exerciseId)).ReturnsAsync((Exercise)null);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _exerciseService.GetByIdAsync(exerciseId));
        }

        [Fact]
        // Перевіряє успішне оновлення існуючої вправи
        public async Task UpdateAsync_ValidUpdate_ReturnsUpdatedExercise()
        {
            // Підготовка
            var exerciseId = "1";
            var existingExercise = new Exercise { Id = exerciseId, Name = "Old Name" };
            var updatedExercise = new Exercise { Id = exerciseId, Name = "New Name", TagsIds = new List<string> { "tag1" } };

            _exerciseRepoMock.Setup(x => x.GetByIdAsync(exerciseId)).ReturnsAsync(existingExercise);
            _exerciseRepoMock.Setup(x => x.ExistsByNameAsync(updatedExercise.Name)).ReturnsAsync(false);
            _exerTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(updatedExercise.TagsIds)).ReturnsAsync(new List<string>());
            _exerciseRepoMock.Setup(x => x.UpdateAsync(exerciseId, updatedExercise)).ReturnsAsync(updatedExercise);

            // Виконання
            var result = await _exerciseService.UpdateAsync(exerciseId, updatedExercise);

            // Перевірка
            result.Should().Be(updatedExercise);
        }

        [Fact]
        // Перевіряє що при оновленні з тією ж назвою не відбувається перевірка на існування
        public async Task UpdateAsync_SameName_DoesNotCheckNameExists()
        {
            // Підготовка
            var exerciseId = "1";
            var existingExercise = new Exercise { Id = exerciseId, Name = "Same Name" };
            var updatedExercise = new Exercise { Id = exerciseId, Name = "Same Name", TagsIds = new List<string> { "tag1" } };

            _exerciseRepoMock.Setup(x => x.GetByIdAsync(exerciseId)).ReturnsAsync(existingExercise);
            _exerTagServiceMock.Setup(x => x.GetNonExistingTagsAsync(updatedExercise.TagsIds)).ReturnsAsync(new List<string>());
            _exerciseRepoMock.Setup(x => x.UpdateAsync(exerciseId, updatedExercise)).ReturnsAsync(updatedExercise);

            // Виконання
            await _exerciseService.UpdateAsync(exerciseId, updatedExercise);

            // Перевірка
            _exerciseRepoMock.Verify(x => x.ExistsByNameAsync(updatedExercise.Name), Times.Never);
        }

        [Fact]
        // Перевіряє помилку при спробі оновити неіснуючу вправу
        public async Task UpdateAsync_NonExistingExercise_ThrowsException()
        {
            // Підготовка
            var exerciseId = "999";
            var updatedExercise = new Exercise { Name = "New Name" };
            _exerciseRepoMock.Setup(x => x.GetByIdAsync(exerciseId)).ReturnsAsync((Exercise)null);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _exerciseService.UpdateAsync(exerciseId, updatedExercise));
        }
    }
}