using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Categories;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentAssertions;
using Moq;

namespace EmphatyWave.Application.Tests.Categories.Commands
{
    public class DeleteCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly DeleteCategoryCommandHandler _handler;

        public DeleteCategoryCommandHandlerTests()
        {
            _categoryRepositoryMock = new();
            _unitMock = new();
            _handler = new DeleteCategoryCommandHandler(_categoryRepositoryMock.Object,_unitMock.Object);
        }

        [Fact]
        public async Task DeteCategoryAsync_Should_ReturnFailure_WhenCategoryNotExists()
        {
            // Arrange
            var command = new DeleteCategoryCommand();
            command.Id = Guid.NewGuid();

            _categoryRepositoryMock.Setup(i => i.GetCategoryById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync((Category)null);
            //act
            var result = await _handler.Handle(command, default);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(CategoryErrors.CategoryNotExists);

        }
        [Fact]
        public async Task DeteCategoryAsync_Should_ReturnFailure_WhenCantSaveChanges()
        {
            // Arrange
            var command = new DeleteCategoryCommand();
            command.Id = Guid.NewGuid();

            _categoryRepositoryMock.Setup(i => i.GetCategoryById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(new Category());
            _unitMock.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
            //act
            var result = await _handler.Handle(command, default);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);

        }
        [Fact]
        public async Task DeteCategoryAsync_Should_ReturnSuccess()
        {
            // Arrange
            var command = new DeleteCategoryCommand();
            command.Id = Guid.NewGuid();
            var category = new Category();
            category.Id = command.Id;
            _categoryRepositoryMock.Setup(i => i.GetCategoryById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(category);
            _unitMock.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
            _categoryRepositoryMock.Setup(i => i.DeleteCategory(category));

            //act
            var result = await _handler.Handle(command, default);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
