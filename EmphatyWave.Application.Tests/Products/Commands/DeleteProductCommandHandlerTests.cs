using CloudinaryDotNet.Actions;
using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Services.Cloudinaries.Abstraction;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Products;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentAssertions;
using Moq;

namespace EmphatyWave.Application.Tests.Products.Commands
{
    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IProductImageRepository> _productImageRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly DeleteProductCommandHandler _handler;

        public DeleteProductCommandHandlerTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _productImageRepositoryMock = new Mock<IProductImageRepository>();
            _unitMock = new Mock<IUnitOfWork>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _handler = new DeleteProductCommandHandler(
                _productRepositoryMock.Object,
                _productImageRepositoryMock.Object,
                _unitMock.Object,
                _cloudinaryServiceMock.Object);
        }

        [Fact]
        public async Task DeleteProductAsync_Should_ReturnFailure_WhenProductNotFound()
        {
            // Arrange
            var command = new DeleteProductCommand { Id = Guid.NewGuid() };

            _productRepositoryMock
                .Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ProductErrors.ProductNotFound);
        }
        
    }
}
