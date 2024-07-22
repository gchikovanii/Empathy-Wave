using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Orders;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentAssertions;
using MediatR;
using Moq;

namespace EmphatyWave.Application.Tests.Orders.Commands
{
    public class UdpateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly UdpateOrderCommandHandler _handler;
        public UdpateOrderCommandHandlerTests()
        {
            _orderRepoMock = new();
            _unitMock = new();
            _handler = new UdpateOrderCommandHandler(_orderRepoMock.Object, _unitMock.Object);
        }

        [Fact]
        public async Task UpdateOrder_Should_ReturnFailure_WhenTryingToDeleteInaccessibleOrder()
        {
            var command = new UpdateOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "tESTuSER" };

            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id)).ReturnsAsync(order);
            _unitMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            var result = await _handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OrderErrors.InaccessibleOrder);
        }
        [Fact]
        public async Task UpdateOrder_Should_ReturnFailure_WhenOrderNotExists()
        {
            var command = new UpdateOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "tESTuSER2" };

            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync((Order)null);
            _unitMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            var result = await _handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OrderErrors.OrderNotExists);
        }
        [Fact]
        public async Task UpdateOrder_Should_ReturnFailure_WhenSaveChangesFailed()
        {
            var command = new UpdateOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "tESTuSER2" };

            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id)).ReturnsAsync(order);
            _unitMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);
        }
        [Fact]
        public async Task UpdateOrder_Should_ReturnSuccess()
        {
            var command = new UpdateOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "tESTuSER2" };

            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id)).ReturnsAsync(order);
            _unitMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = await _handler.Handle(command, default);
            result.IsSuccess.Should().BeTrue();
            _unitMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
