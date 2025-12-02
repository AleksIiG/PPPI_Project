using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.WorkoutTagDtos;
using api.Helpers;
using api.Interface;
using api.Models;
using api.Services;
using Moq;
using Xunit;
using FluentAssertions;

namespace Tests.Services
{
    public class WorkoutTagServiceTests
    {
        private readonly Mock<IWorkoutTagRepository> _workoutTagRepoMock;
        private readonly WorkoutTagService _workoutTagService;

        public WorkoutTagServiceTests()
        {
            // Створюємо mock-об'єкт для репозиторію тегів тренувань
            _workoutTagRepoMock = new Mock<IWorkoutTagRepository>();
            _workoutTagService = new WorkoutTagService(_workoutTagRepoMock.Object);
        }

        [Fact]
        // Перевіряє отримання DTO тегів за списком ID з тренувань
        public async Task GetByIdsFromWorkoutsAsync_ShouldReturnMappedDtos()
        {
            // Підготовка
            var tagIds = new List<string> { "1", "2" };
            var tags = new List<WorkoutTag>
            {
                new WorkoutTag { Id = "1", Name = "Tag 1" },
                new WorkoutTag { Id = "2", Name = "Tag 2" }
            };
            _workoutTagRepoMock.Setup(x => x.GetByIdsFromWorkoutsAsync(tagIds))
                             .ReturnsAsync(tags);

            // Виконання
            var result = await _workoutTagService.GetByIdsFromWorkoutsAsync(tagIds);

            // Перевірка
            result.Should().HaveCount(2);
            result[0].Id.Should().Be("1");
            result[0].Name.Should().Be("Tag 1");
        }

        [Fact]
        // Перевіряє отримання списку неіснуючих тегів
        public async Task GetNonExistingTagsAsync_ShouldReturnMissingTags()
        {
            // Підготовка
            var tagIds = new List<string> { "1", "2", "3" };
            var existingTags = new List<string> { "1", "2" };
            _workoutTagRepoMock.Setup(x => x.GetNonExistingTagsAsync(tagIds))
                             .ReturnsAsync(existingTags);

            // Виконання
            var result = await _workoutTagService.GetNonExistingTagsAsync(tagIds);

            // Перевірка
            result.Should().ContainSingle();
            result[0].Should().Be("3");
        }

        [Fact]
        // Перевіряє отримання всіх тегів з фільтрацією
        public async Task GetAllAsync_ShouldReturnTags()
        {
            // Підготовка
            var query = new QueryObjectForTags();
            var tags = new List<WorkoutTag>
            {
                new WorkoutTag { Id = "1", Name = "Tag 1" }
            };
            _workoutTagRepoMock.Setup(x => x.GetAllAsync(query))
                             .ReturnsAsync(tags);

            // Виконання
            var result = await _workoutTagService.GetAllAsync(query);

            // Перевірка
            result.Should().ContainSingle();
        }

        [Fact]
        // Перевіряє отримання тегу за існуючим ID
        public async Task GetByIdAsync_ShouldReturnTag()
        {
            // Підготовка
            var tagId = "1";
            var tag = new WorkoutTag { Id = tagId, Name = "Tag 1" };
            _workoutTagRepoMock.Setup(x => x.GetByIdAsync(tagId))
                             .ReturnsAsync(tag);

            // Виконання
            var result = await _workoutTagService.GetByIdAsync(tagId);

            // Перевірка
            result.Should().Be(tag);
        }

        [Fact]
        // Перевіряє успішне створення нового тегу
        public async Task CreateAsync_ShouldReturnCreatedTag()
        {
            // Підготовка
            var tag = new WorkoutTag { Name = "New Tag" };
            var createdTag = new WorkoutTag { Id = "1", Name = "New Tag" };
            _workoutTagRepoMock.Setup(x => x.CreateAsync(tag))
                             .ReturnsAsync(createdTag);

            // Виконання
            var result = await _workoutTagService.CreateAsync(tag);

            // Перевірка
            result.Should().Be(createdTag);
        }

        [Fact]
        // Перевіряє успішне оновлення тегу
        public async Task UpdateAsync_ShouldReturnUpdatedTag()
        {
            // Підготовка
            var tagId = "1";
            var tag = new WorkoutTag { Name = "Updated Tag" };

            // Мокуємо метод репозиторію так, щоб він повертав той самий об'єкт, який отримав
            _workoutTagRepoMock.Setup(x => x.UpdateAsync(It.IsAny<WorkoutTag>(), It.IsAny<string>()))
                             .ReturnsAsync((WorkoutTag t, string id) => t);

            // Виконання
            var result = await _workoutTagService.UpdateAsync(tag, tagId);

            // Перевірка
            result.Should().Be(tag); // Перевіряємо, що повернуто той самий об'єкт
            result.Id.Should().Be(tagId); // Перевіряємо, що ID встановлено коректно
            result.Name.Should().Be("Updated Tag");
        }

        [Fact]
        // Перевіряє успішне видалення тегу
        public async Task DeleteAsync_ShouldReturnDeletedTag()
        {
            // Підготовка
            var tagId = "1";
            var tag = new WorkoutTag { Id = tagId, Name = "Tag 1" };
            _workoutTagRepoMock.Setup(x => x.GetByIdAsync(tagId))
                             .ReturnsAsync(tag);
            _workoutTagRepoMock.Setup(x => x.DeleteAsync(tagId))
                             .ReturnsAsync(tag);

            // Виконання
            var result = await _workoutTagService.DeleteAsync(tagId);

            // Перевірка
            result.Should().Be(tag);
        }

        [Fact]
        // Перевіряє помилку при спробі видалити неіснуючий тег
        public async Task DeleteAsync_ShouldThrowException_WhenTagNotFound()
        {
            // Підготовка
            var tagId = "999";
            _workoutTagRepoMock.Setup(x => x.GetByIdAsync(tagId))
                             .ReturnsAsync((WorkoutTag)null);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _workoutTagService.DeleteAsync(tagId)
            );
        }
    }
}