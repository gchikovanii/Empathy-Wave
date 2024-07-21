using EmphatyWave.Application.Queries.Categories;
using EmphatyWave.Application.Queries.Categories.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using FluentAssertions;
using Moq;

namespace EmphatyWave.Application.Tests.Categories.Queries
{
    public class GetCategoriesQueryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly GetCategoriesQueryHandler _handler;
        public GetCategoriesQueryHandlerTests()
        {
            _categoryRepoMock = new();
            _handler = new GetCategoriesQueryHandler(_categoryRepoMock.Object);
        }
        [Fact]
        public async Task Handle_Should_ReturnEmptyCollection_WhenNoCategories()
        {
            //Arrange
            var query = new GetCategoriesQuery();
            var emptyList = new List<Category>();

            _categoryRepoMock
                .Setup(repo => repo.GetCategories(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);
            // Act
            var result = await _handler.Handle(query, default);
            // Assert
            result.Should().BeEmpty();
        }
        [Fact]
        public async Task GetCategoriesAsync_Should_ReturnCategoriesList()
        {
            //Arrange
            var query = new GetCategoriesQuery();
            var categoryList = new List<Category>()
            {
                new (){Id = Guid.NewGuid(),Name = "First Product"},
                new (){Id = Guid.NewGuid(),Name = "Second Product"},
            };
            var categoryDtos = categoryList.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList();

            _categoryRepoMock
                .Setup(repo => repo.GetCategories(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoryList);
            // Act
            var result = await _handler.Handle(query, default);
            // Assert
            result.Should().BeEquivalentTo(categoryDtos);
        }
    }
}
