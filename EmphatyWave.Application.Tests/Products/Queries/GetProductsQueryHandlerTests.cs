using EmphatyWave.Application.Queries.Categories.DTOs;
using EmphatyWave.Application.Queries.Categories;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Moq;
using EmphatyWave.Application.Queries.Products;
using FluentAssertions;
using EmphatyWave.Application.Queries.Products.DTOs;

namespace EmphatyWave.Application.Tests.Products.Queries
{
    public class GetProductsQueryHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly GetProductsQueryHandler _handler;
        public GetProductsQueryHandlerTests()
        {
            _productRepoMock = new();
            _handler = new GetProductsQueryHandler(_productRepoMock.Object);
        }
        [Fact]
        public async Task GetProductsAync_Should_ReturnEmptyCollection_WhenNoProducts()
        {
            //Arrange
            var query = new GetProductsQuery();
            var emptyList = new List<Product>();

            _productRepoMock
                .Setup(repo => repo.GetProducts(It.IsAny<CancellationToken>(),1,1))
                .ReturnsAsync(emptyList);
            // Act
            var result = await _handler.Handle(query, default);
            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task GetProductsAsync_Should_ReturnProductsList()
        {
            // Arrange
            var query = new GetProductsQuery { PageNumber = 1, PageSize = 1 };
            var productList = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "First Product", SKU = "c.SKU", Description = "c.Description", CategoryId = Guid.NewGuid(), Discount = 0, Price = 15, Title = "c.Title" },
            new Product { Id = Guid.NewGuid(), Name = "Second Product", SKU = "c.SKU", Description = "c.Description", CategoryId = Guid.NewGuid(), Discount = 0, Price = 15, Title = "c.Title" }
        };
            var productDtos = productList.Select(c => new ProductDto
            {
                Id = c.Id,
                Name = c.Name,
                SKU = c.SKU,
                Description = c.Description,
                CategoryId = c.CategoryId,
                Discount = c.Discount,
                Price = c.Price,
                Title = c.Title
            }).ToList();

            _productRepoMock
                .Setup(repo => repo.GetProducts(It.IsAny<CancellationToken>(), query.PageNumber, query.PageSize))
                .ReturnsAsync(productList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(productDtos);
        }
    }
}
