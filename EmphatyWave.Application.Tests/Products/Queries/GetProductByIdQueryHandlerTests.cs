using EmphatyWave.Application.Queries.Products.DTOs;
using EmphatyWave.Application.Queries.Products;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Moq;
using EmphatyWave.Domain;
using Mapster;
using FluentAssertions;

namespace EmphatyWave.Application.Tests.Products.Queries
{
    public class GetProductByIdQueryHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly GetProductByIdQueryHandler _handler;

        public GetProductByIdQueryHandlerTests()
        {
            _productRepoMock = new Mock<IProductRepository>();
            _handler = new GetProductByIdQueryHandler(_productRepoMock.Object);
        }

        [Fact]
        public async Task GetProductByIdAsync_Should_ReturnProductDto_WhenProductExists()
        {
            // Arrange
            var query = new GetProductByIdQuery { Id = Guid.NewGuid() };
            var product = new Product
            {
                Id = query.Id,
                Name = "Product Name",
                SKU = "Product SKU",
                Description = "Product Description",
                CategoryId = Guid.NewGuid(),
                Discount = 10,
                Price = 100,
                Title = "Product Title"
            };

            var expectedProductDto = product.Adapt<ProductDto>();

            _productRepoMock
                .Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), query.Id))
                .ReturnsAsync(product);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedProductDto);
        }

        [Fact]
        public async Task GetProductByIdAsync_Should_ReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var query = new GetProductByIdQuery { Id = Guid.NewGuid() };

            _productRepoMock
                .Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), query.Id))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
