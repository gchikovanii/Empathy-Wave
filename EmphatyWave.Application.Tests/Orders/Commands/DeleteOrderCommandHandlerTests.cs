using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Orders;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentAssertions;
using Moq;

namespace EmphatyWave.Application.Tests.Orders.Commands
{
    public class DeleteOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepoMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly DeleteOrderCommandHandler _handler;

        public DeleteOrderCommandHandlerTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderItemRepoMock = new Mock<IOrderItemRepository>();
            _unitMock = new Mock<IUnitOfWork>();
            _handler = new DeleteOrderCommandHandler(_orderRepoMock.Object, _orderItemRepoMock.Object, _unitMock.Object);
        }

        [Fact]
        public async Task DeleteOrder_Should_ReturnFailure_WhenTryingToDeleteInaccessibleOrder()
        {
            var command = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "tESTuSER" };

            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id)).ReturnsAsync(order);


            _orderItemRepoMock.Verify(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), It.IsAny<Guid>()), Times.Never);
            _unitMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            var result = await _handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OrderErrors.InaccessibleOrder);
        }
       
        [Fact]
        public async Task DeleteOrder_Should_ReturnFailure_WhenOrderNotFound()
        {
            var command = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };
            var newOrder = new Order { Id = Guid.Empty, UserId = "tESTuSER2" };
            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync((Order)null);

            var result = await _handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OrderErrors.OrderNotExists);

            _orderItemRepoMock.Verify(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), It.IsAny<Guid>()), Times.Never);
            _unitMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOrder_Should_ReturnFailure_WhenOrderItemsNotFound()
        {
            var command = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };
            var emptyOrder = new Order { Id = Guid.Empty, UserId = "tESTuSER2" };
            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(emptyOrder);

            _orderItemRepoMock.Verify(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), It.IsAny<Guid>()), Times.Never);
            _unitMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            var result = await _handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OrderErrors.OrderItemsNotExist);
        }

        [Fact]
        public async Task DeleteOrder_Should_ReturnFailure_OnSaveFailure()
        {
            var common = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "TestUserId" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "TestUserId" };
            var orderItems = new List<OrderItem>() { new OrderItem() };

            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), common.Id)).ReturnsAsync(order);
            _orderItemRepoMock.Setup(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), common.Id)).ReturnsAsync(orderItems);
            _unitMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _handler.Handle(common, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);

        }

        [Fact]
        public async Task DeleteOrder_Should_ReturnSuccess()
        {
            var common = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "TestUserId" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "TestUserId" };
            var orderItems = new List<OrderItem>() { new OrderItem() };

            _orderRepoMock.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), common.Id)).ReturnsAsync(order);
            _orderItemRepoMock.Setup(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), common.Id)).ReturnsAsync(orderItems);
            _unitMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _handler.Handle(common, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
