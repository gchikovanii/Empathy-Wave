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
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<IOrderItemRepository> _mockOrderItemRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly DeleteOrderCommandHandler _handler;

        public DeleteOrderCommandHandlerTests()
        {
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockOrderItemRepo = new Mock<IOrderItemRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _handler = new DeleteOrderCommandHandler(_mockOrderRepo.Object, _mockOrderItemRepo.Object, _mockUnitOfWork.Object);
        }


        [Fact]
        public async Task DeleteOrder_Should_ReturnFailure_WhenTryingToDeleteInaccessibleOrder()
        {
            var handler = new DeleteOrderCommandHandler(_mockOrderRepo.Object, _mockOrderItemRepo.Object, _mockUnitOfWork.Object);
            var command = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "tESTuSER" };

            _mockOrderRepo.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id)).ReturnsAsync(order);


            _mockOrderItemRepo.Verify(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), It.IsAny<Guid>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            var result = await _handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OrderErrors.InaccessibleOrder);
        }
       
        [Fact]
        public async Task DeleteOrder_Should_ReturnFailure_WhenOrderNotFound()
        {
            var handler = new DeleteOrderCommandHandler(_mockOrderRepo.Object, _mockOrderItemRepo.Object, _mockUnitOfWork.Object);
            var command = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };

            var emptyOrder = new Order { Id = Guid.Empty, UserId = string.Empty };
            _mockOrderRepo.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(emptyOrder);

            _mockOrderItemRepo.Verify(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), It.IsAny<Guid>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            var result = await handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OrderErrors.OrderNotExists);
        }

        [Fact]
        public async Task DeleteOrder_Should_ReturnFailure_WhenOrderItemsNotFound()
        {
            var handler = new DeleteOrderCommandHandler(_mockOrderRepo.Object, _mockOrderItemRepo.Object, _mockUnitOfWork.Object);
            var command = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "tESTuSER2" };

            var emptyOrder = new Order { Id = Guid.Empty, UserId = "tESTuSER2" };
            _mockOrderRepo.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), command.Id))
                .ReturnsAsync(emptyOrder);

            _mockOrderItemRepo.Verify(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), It.IsAny<Guid>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            var result = await handler.Handle(command, default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(OrderErrors.OrderItemsNotExist);
        }

        [Fact]
        public async Task DeleteOrder_Should_ReturnFailure_OnSaveFailure()
        {
            var handler = new DeleteOrderCommandHandler(_mockOrderRepo.Object, _mockOrderItemRepo.Object, _mockUnitOfWork.Object);
            var common = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "TestUserId" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "TestUserId" };
            var orderItems = new List<OrderItem>() { new OrderItem() };

            _mockOrderRepo.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), common.Id)).ReturnsAsync(order);
            _mockOrderItemRepo.Setup(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), common.Id)).ReturnsAsync(orderItems);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await handler.Handle(common, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);

        }

        [Fact]
        public async Task DeleteOrder_Should_ReturnSuccess()
        {
            var handler = new DeleteOrderCommandHandler(_mockOrderRepo.Object, _mockOrderItemRepo.Object, _mockUnitOfWork.Object);
            var common = new DeleteOrderCommand { Id = Guid.NewGuid(), UserId = "TestUserId" };
            var order = new Order { Id = Guid.NewGuid(), UserId = "TestUserId" };
            var orderItems = new List<OrderItem>() { new OrderItem() };

            _mockOrderRepo.Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), common.Id)).ReturnsAsync(order);
            _mockOrderItemRepo.Setup(repo => repo.GetOrderItems(It.IsAny<CancellationToken>(), common.Id)).ReturnsAsync(orderItems);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await handler.Handle(common, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
