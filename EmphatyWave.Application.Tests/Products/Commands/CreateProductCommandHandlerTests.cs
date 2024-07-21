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
        public async Task CreateProductAsync_Should_ReturnFailure_WhenpRroductAlreadyExists()
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
            _productRepositoryMock.Setup(i => i.GetProductByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync(new Product());
            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be("Product.AlreadyExists");
            result.Error.descripton.Should().Be("Already Exists");
        }

    }
}
