using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Categories;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace EmphatyWave.Application.Tests.Categories.Commands
{
    public class CreateCategoryCommandHandlertTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IValidator<CreateCategoryCommand>> _validatorMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly CreateCategoryCommandHandler _handler;

        public CreateCategoryCommandHandlertTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _validatorMock = new Mock<IValidator<CreateCategoryCommand>>();
            _unitMock = new Mock<IUnitOfWork>();
            _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock.Object, _unitMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task CreateCategoryAsync_Should_ReturnFailure_WhenCategoryAlreadyExists()
        {
            // Arrange
            var command = new CreateCategoryCommand { Name = "SameCategory" };

            // Mock the category repository to return a category indicating it already exists
            _categoryRepositoryMock
                .Setup(repo => repo.GetCategoryByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync(new Category());

            // Mock the validator to return a valid result
            _validatorMock
                .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(CategoryErrors.AlreadyExists);
        }
        [Fact]
        public async Task CreateCategoryAsync_Should_ReturnValidationFailure_WhenIncorrectDataIsInputed()
        {
            // Arrange
            var command = new CreateCategoryCommand { Name = "CategoryName" };
           

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Category name is invalid.")
            });
            _validatorMock
               .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
               .ReturnsAsync(validationResult);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be("ValidationError");
            result.Error.descripton.Should().Contain("Category name is invalid.");
        }
        [Fact]
        public async Task CreateCategoryAsync_Should_ReturnFailure()
        {
            //Arrange
            var command = new CreateCategoryCommand { Name = "NewCategoryName" };
            _categoryRepositoryMock.Setup(i => i.GetCategoryByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync((Category)null);

            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _categoryRepositoryMock.Setup(i => i.CreateCategoryAsync(It.IsAny<CancellationToken>(), It.IsAny<Category>()))
                .Returns(Task.CompletedTask);
            _unitMock.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
            
            //Act
            var result = await _handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);
        }
        [Fact]
        public async Task CreateCategoryAsync_Should_ReturnSuccess()
        {
            //Arrange
            var command = new CreateCategoryCommand { Name = "NewCategoryName" };
            _categoryRepositoryMock.Setup(i => i.GetCategoryByName(It.IsAny<CancellationToken>(), command.Name))
                .ReturnsAsync((Category)null);

            _validatorMock.Setup(i => i.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _unitMock.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
            _categoryRepositoryMock.Setup(i => i.CreateCategoryAsync(It.IsAny<CancellationToken>(),It.IsAny<Category>()))
                .Returns(Task.CompletedTask);   
            //Act
            var result = await _handler.Handle(command, default);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
