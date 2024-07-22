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
using System.Data;

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
        [Fact]
        public async Task DeleteProductAsync_Should_ReturnFailure_WhenSaveChangesFailed()
        {
            // Arrange
            var command = new DeleteProductCommand { Id = Guid.NewGuid() };
            var product = new Product { Id = command.Id };
            var images = new List<ProductImage>
                {
                    new ProductImage { Id = Guid.NewGuid(), PublicId = "testguid1" },
                    new ProductImage { Id = Guid.NewGuid(), PublicId = "testguid2" }
                };
            _productRepositoryMock
                .Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(product);
            _productImageRepositoryMock
               .Setup(repo => repo.GetImages(It.IsAny<CancellationToken>(), command.Id))
               .ReturnsAsync(images);
            _unitMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var transactionMock = new Mock<IDbTransaction>();
            _unitMock
                .Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);
            transactionMock.Verify(t => t.Rollback(), Times.Once);
        }
        [Fact]
        public async Task DeleteProductAsync_Should_ReturnFailure_WhenExceptionIsThrown()
        {
            // Arrange
            var command = new DeleteProductCommand { Id = Guid.NewGuid() };
            var product = new Product { Id = command.Id };
            var images = new List<ProductImage>
                {
                    new ProductImage { Id = Guid.NewGuid(), PublicId = "testguid1" },
                    new ProductImage { Id = Guid.NewGuid(), PublicId = "testguid2" }
                };

            _productRepositoryMock
                .Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(product);

            _productImageRepositoryMock
                .Setup(repo => repo.GetImages(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(images);

            _unitMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var transactionMock = new Mock<IDbTransaction>();
            _unitMock
                .Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(new Persistence.Infrastructure.ErrorsAggregate.Common.Error("Unexpected Exception", "Unexpected error"));

            transactionMock.Verify(t => t.Rollback(), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_Should_ReturnSuccess()
        {
            // Arrange
            var command = new DeleteProductCommand { Id = Guid.NewGuid() };
            var product = new Product { Id = command.Id };
            var images = new List<ProductImage>
            {
                new ProductImage { Id = Guid.NewGuid(), PublicId = "public-id-1" },
                new ProductImage { Id = Guid.NewGuid(), PublicId = "public-id-2" }
            };

            _productRepositoryMock
                .Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(product);

            _productImageRepositoryMock
                .Setup(repo => repo.GetImages(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(images);

            _unitMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var transactionMock = new Mock<IDbTransaction>();
            _unitMock
                .Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            transactionMock.Verify(t => t.Commit(), Times.Once);
            _productImageRepositoryMock.Verify(repo => repo.DeleteProductImage(It.IsAny<CancellationToken>(), It.IsAny<Guid>()), Times.Exactly(images.Count));
            _cloudinaryServiceMock.Verify(service => service.DeleteImage(It.IsAny<string>()), Times.Exactly(images.Count));
        }

    }
}
