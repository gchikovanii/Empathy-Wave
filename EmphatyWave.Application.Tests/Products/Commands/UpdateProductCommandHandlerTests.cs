using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Products;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace EmphatyWave.Application.Tests.Products.Commands
{
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly Mock<IValidator<UpdateProductCommand>> _validatorMock;
        private readonly UpdateProductCommandHandler _handler;
        public UpdateProductCommandHandlerTests()
        {
            _productRepoMock = new();
            _unitMock = new();
            _validatorMock = new();
            _handler = new UpdateProductCommandHandler(_productRepoMock.Object, _validatorMock.Object, _unitMock.Object);
        }

        [Fact]
        public async Task UpdateProductAsync_Should_ReturnFailure_WhenIncorrectDataIsInputed()
        {
            // Arrange
            var command = new UpdateProductCommand
            {
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Updated Product Name",
                Quantity = 15,
                Title = "Updated Product Title"
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
        public async Task UpdateProductAsync_Should_ReturnFailure_WhenProductNotFound()
        {
            // Arrange
            var command = new UpdateProductCommand
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

            _productRepoMock.Setup(i => i.GetProductByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync((Product)null);
            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be(ProductErrors.ProductNotFound.code);
            result.Error.descripton.Should().Be(ProductErrors.ProductNotFound.descripton);
        }

        [Fact]
        public async Task UpdateProductAsync_Should_ReturnFailure_WhenSaveChangesFailed()
        {
            // Arrange
            var command = new UpdateProductCommand
            {
                Id = Guid.NewGuid(),
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title"
            };

            Product existingProduct = new Product
            {
                Id = Guid.NewGuid(),
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title"
            };

            _productRepoMock.Setup(i => i.GetProductById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(existingProduct);
            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());
            _unitMock.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);
        }
        [Fact]
        public async Task UpdateProductAsync_Should_ReturnSuccess()
        {
            // Arrange
            var command = new UpdateProductCommand
            {
                Id = Guid.NewGuid(),
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title"
            };

            Product existingProduct = new Product
            {
                Id = Guid.NewGuid(),
                SKU = "ASD332",
                CategoryId = Guid.NewGuid(),
                Description = "asdasdasdasdStringassd",
                Discount = 0,
                Price = 15,
                Name = "Product Name",
                Quantity = 30,
                Title = "Product Title"
            };

            _productRepoMock.Setup(i => i.GetProductById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(existingProduct);
            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult());
            _unitMock.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _productRepoMock.Verify(r => r.UpdateProduct(It.Is<Product>(p =>
                p.Id == command.Id &&
                p.SKU == command.SKU &&
                p.CategoryId == command.CategoryId &&
                p.Description == command.Description &&
                p.Discount == command.Discount &&
                p.Price == command.Price &&
                p.Name == command.Name &&
                p.Quantity == command.Quantity &&
                p.Title == command.Title
            )), Times.Once);
        }
    }
}
