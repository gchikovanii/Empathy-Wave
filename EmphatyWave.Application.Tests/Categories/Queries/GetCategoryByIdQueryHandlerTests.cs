using EmphatyWave.Application.Queries.Categories;
using EmphatyWave.Application.Queries.Categories.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using FluentAssertions;
using Moq;

namespace EmphatyWave.Application.Tests.Categories.Queries
{
    public class GetCategoryByIdQueryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly GetCategoryByIdQueryHandler _handler;
        public GetCategoryByIdQueryHandlerTests()
        {
            _categoryRepoMock = new();
            _handler = new GetCategoryByIdQueryHandler(_categoryRepoMock.Object);
        }

        [Fact]
        public async Task GetCategoryById_Should_ReturnNull_WhenCategoryNotFound()
        {
            // Arrange
            var query = new GetCategoryByIdQuery { Id = Guid.NewGuid() };

            _categoryRepoMock
                .Setup(repo => repo.GetCategoryById(It.IsAny<CancellationToken>(), query.Id))
                .ReturnsAsync((Category)null);
            // Act
            var result = await _handler.Handle(query, default);
            // Assert
            result.Should().BeNull(); // or check for an appropriate return value
        }

        [Fact]
        public async Task GetCategoryById_Should_ReturnMappedCategoryDto()
        {
            // Arrange
            var query = new GetCategoryByIdQuery { Id = Guid.NewGuid() };
            var category = new Category { Id = query.Id, Name = "CategoryName" };
            var categoryDto = new CategoryDto { Id = category.Id, Name = category.Name };

            _categoryRepoMock
                .Setup(repo => repo.GetCategoryById(It.IsAny<CancellationToken>(), query.Id))
                .ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, default);

            // Assert
            result.Should().BeEquivalentTo(categoryDto);
        }
    }
}
