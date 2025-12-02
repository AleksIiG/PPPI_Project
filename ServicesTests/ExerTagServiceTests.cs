using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerTagsDto;
using api.Helpers;
using api.Interface;
using api.Models;
using api.Services;
using Moq;
using Xunit;
using FluentAssertions;

namespace Tests.Services
{
    public class ExerTagServiceTests
    {
        private readonly Mock<IExerTagRepository> _repoMock = new();
        private readonly ExerTagService _service;

        public ExerTagServiceTests()
        {
            // Створюємо сервіс тегів вправ з mock-репозиторієм
            _service = new ExerTagService(_repoMock.Object);
        }

        [Fact]
        // Перевіряє отримання DTO тегів за списком ID з вправ
        public async Task GetByIdsFromExercisesAsync_ReturnsMappedDtos()
        {
            // Підготовка
            var tags = new List<ExerciseTag> { new() { Id = "1", Name = "Tag 1" } };
            _repoMock.Setup(x => x.GetByIdsFromExercisesAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(tags);

            // Виконання
            var result = await _service.GetByIdsFromExercisesAsync(new List<string> { "1" });

            // Перевірка
            result.Should().ContainSingle();
            result[0].Name.Should().Be("Tag 1");
        }

        [Fact]
        // Перевіряє отримання списку неіснуючих тегів
        public async Task GetNonExistingTagsAsync_ReturnsMissingIds()
        {
            // Підготовка
            var tagIds = new List<string> { "1", "2", "3" };
            _repoMock.Setup(x => x.GetNonExistingTagsAsync(tagIds)).ReturnsAsync(new List<string> { "1", "2" });

            // Виконання
            var result = await _service.GetNonExistingTagsAsync(tagIds);

            // Перевірка
            result.Should().ContainSingle();
            result[0].Should().Be("3");
        }

        [Fact]
        // Перевіряє отримання всіх тегів з фільтрацією
        public async Task GetAllAsync_ReturnsTags()
        {
            // Підготовка
            var tags = new List<ExerciseTag> { new() { Id = "1" } };
            _repoMock.Setup(x => x.GetAllAsync(It.IsAny<QueryObjectForTags>())).ReturnsAsync(tags);

            // Виконання
            var result = await _service.GetAllAsync(new QueryObjectForTags());

            // Перевірка
            result.Should().ContainSingle();
        }

        [Fact]
        // Перевіряє отримання тегу за існуючим ID
        public async Task GetByIdAsync_ReturnsTag()
        {
            // Підготовка
            var tag = new ExerciseTag { Id = "1" };
            _repoMock.Setup(x => x.GetByIdAsync("1")).ReturnsAsync(tag);

            // Виконання
            var result = await _service.GetByIdAsync("1");

            // Перевірка
            result.Should().Be(tag);
        }

        [Fact]
        // Перевіряє успішне створення нового тегу
        public async Task CreateAsync_ReturnsCreatedTag()
        {
            // Підготовка
            var tag = new ExerciseTag { Name = "New Tag" };
            var created = new ExerciseTag { Id = "1", Name = "New Tag" };
            _repoMock.Setup(x => x.CreateAsync(tag)).ReturnsAsync(created);

            // Виконання
            var result = await _service.CreateAsync(tag);

            // Перевірка
            result.Should().Be(created);
        }

        [Fact]
        // Перевіряє успішне оновлення існуючого тегу
        public async Task UpdateAsync_ReturnsUpdatedTagWithId()
        {
            // Підготовка
            var updatedTag = new ExerciseTag { Id = "1", Name = "Updated" };
            _repoMock.Setup(x => x.UpdateAsync(It.IsAny<ExerciseTag>(), "1"))
                     .ReturnsAsync(updatedTag);

            // Виконання
            var result = await _service.UpdateAsync(new ExerciseTag { Name = "Updated" }, "1");

            // Перевірка
            result.Id.Should().Be("1");
            result.Name.Should().Be("Updated");
        }

        [Fact]
        // Перевіряє успішне видалення існуючого тегу
        public async Task DeleteAsync_Existing_ReturnsTag()
        {
            // Підготовка
            var tag = new ExerciseTag { Id = "1" };
            _repoMock.Setup(x => x.GetByIdAsync("1")).ReturnsAsync(tag);
            _repoMock.Setup(x => x.DeleteAsync("1")).ReturnsAsync(tag);

            // Виконання
            var result = await _service.DeleteAsync("1");

            // Перевірка
            result.Should().Be(tag);
        }

        [Fact]
        // Перевіряє помилку при спробі видалити неіснуючий тег
        public async Task DeleteAsync_NonExisting_Throws()
        {
            // Підготовка
            _repoMock.Setup(x => x.GetByIdAsync("999")).ReturnsAsync((ExerciseTag)null);

            // Виконання & Перевірка
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync("999"));
        }
    }
}