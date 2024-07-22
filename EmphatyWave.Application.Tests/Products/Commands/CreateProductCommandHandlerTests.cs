using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Services.PromoCodeImages.Abstraction;
using EmphatyWave.Application.Services.PromoCodeImages.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Categories;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;

namespace EmphatyWave.Application.Tests.Products.Commands
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IProductImageService> _productImageMockService;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly Mock<IValidator<CreateProductCommand>> _validatorMock;
        private readonly CreateProductCommandHandler _handler;
        public CreateProductCommandHandlerTests()
        {
            _productImageMockService = new();
            _categoryRepoMock = new();
            _productRepositoryMock = new();
            _loggerMock = new();
            _unitMock = new();
            _validatorMock = new();
            _handler = new CreateProductCommandHandler(_productRepositoryMock.Object, _categoryRepoMock.Object, _validatorMock.Object,
                _unitMock.Object, _productImageMockService.Object, _loggerMock.Object);
        }
        [Fact]
        public async Task CreateProductAsync_Should_ReturnFailure_WhenCategoryNotExists()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdassd"
,
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title"
            };

            // Mock the category repository to return a category indicating it already exists
            _categoryRepoMock
                .Setup(repo => repo.GetCategoryById(It.IsAny<CancellationToken>(), command.CategoryId))
                .ReturnsAsync((Category)null);
            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(CategoryErrors.CategoryNotExists);
        }
        [Fact]
        public async Task CreateProductAsync_Should_ReturnFailure_WhenIncorrectDataIsInputed()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title"
            };

            var validationResult = new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("Name", "Incorrect data.")
                });

            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be("ValidationError");
            result.Error.descripton.Should().Contain("Incorrect data.");
        }
        [Fact]
        public async Task CreateProductAsync_Should_ReturnFailure_WhenProductAlreadyExists()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title"
            };
            _categoryRepoMock
               .Setup(repo => repo.GetCategoryById(It.IsAny<CancellationToken>(), command.CategoryId))
               .ReturnsAsync(new Category());
            Product existingProduct = new Product
            {
                Id = Guid.NewGuid(),
                SKU = "ASD332",
                CategoryId = command.CategoryId,
                Description = "Existing product description",
                Discount = 5,
                Price = 20,
                Name = "Existing Product Name",
                Quantity = 100,
                Title = "Existing Product Title"
            };

            _productRepositoryMock.Setup(i => i.GetProductByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync(existingProduct);
            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be("Product.AlreadyExists");
            result.Error.descripton.Should().Be("Already Exists");
        }

        [Fact]
        public async Task CreateProductAsync_Should_ReturnFailure_WhenImageUploadFails()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title",
                Images = new List<string> { "base64String" } // Mock image data
            };

            var mockTransaction = new Mock<IDbTransaction>();
            _unitMock
                .Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            _categoryRepoMock
               .Setup(repo => repo.GetCategoryById(It.IsAny<CancellationToken>(), command.CategoryId))
               .ReturnsAsync(new Category());

            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            _productRepositoryMock.Setup(i => i.GetProductByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync((Product)null);
            _productImageMockService
                .Setup(service => service.UplaodImages(It.IsAny<CancellationToken>(), It.IsAny<List<CreateProductImageDto>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be("Image.UploadError");
            result.Error.descripton.Should().Be("Image hasn't uploaded, but product added successfully");

            // Verify rollback was called
            mockTransaction.Verify(t => t.Rollback(), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_Should_ReturnFailure_WhenSaveChangesFailed()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title",
                Images = new List<string> { "base64String" } // Mock image data
            };

            var mockTransaction = new Mock<IDbTransaction>();
            _unitMock
                .Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            _categoryRepoMock
               .Setup(repo => repo.GetCategoryById(It.IsAny<CancellationToken>(), command.CategoryId))
               .ReturnsAsync(new Category());

            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            _productRepositoryMock.Setup(i => i.GetProductByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync((Product)null);
            _productImageMockService
                .Setup(service => service.UplaodImages(It.IsAny<CancellationToken>(), It.IsAny<List<CreateProductImageDto>>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);
            _unitMock.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
            mockTransaction.Verify(t => t.Rollback(), Times.Once);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);
        }



        [Fact]
        public async Task CreateProductAsync_Should_ReturnSuccess()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title",
                Images = new List<string> { "base64String" } 
            };

            var mockTransaction = new Mock<IDbTransaction>();
            _unitMock
                .Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            _categoryRepoMock
               .Setup(repo => repo.GetCategoryById(It.IsAny<CancellationToken>(), command.CategoryId))
               .ReturnsAsync(new Category());

            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            _productRepositoryMock.Setup(i => i.GetProductByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync((Product)null); 

            _productImageMockService
                .Setup(service => service.UplaodImages(It.IsAny<CancellationToken>(), It.IsAny<List<CreateProductImageDto>>()))
                .ReturnsAsync(true);

            _unitMock.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true); 
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            // Assert
            result.IsSuccess.Should().BeTrue();
            mockTransaction.Verify(t => t.Commit(), Times.Once);
        }
    }
}
