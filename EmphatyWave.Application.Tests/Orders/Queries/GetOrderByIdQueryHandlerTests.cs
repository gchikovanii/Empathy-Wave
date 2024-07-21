using EmphatyWave.Application.Queries.Orders.DTOs;
using EmphatyWave.Application.Queries.Orders;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using FluentAssertions;
using Moq;
using Mapster;

namespace EmphatyWave.Application.Tests.Orders.Queries
{
    public class GetOrderByIdQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly GetOrderByIdQueryHandler _handler;

        public GetOrderByIdQueryHandlerTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _handler = new GetOrderByIdQueryHandler(_orderRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnOrderDto_WhenOrderExists()
        {
            // Arrange
            var query = new GetOrderByIdQuery { Id = Guid.NewGuid() };

            var order = new Order
            {
                Id = query.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                OrderItems = new List<OrderItem>(),
                TotalAmount = 100,
                ShippingDetails = new ShippingDetail(),
                Status = Status.PaymentPending,
                UserId = "user1",
                StripeToken = "token123",
                User = new User()
            };

            var expectedOrderDto = order.Adapt<OrderDto>();

            _orderRepoMock
                .Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), query.Id))
                .ReturnsAsync(order);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedOrderDto);
        }

        [Fact]
        public async Task Handle_Should_ReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            var query = new GetOrderByIdQuery { Id = Guid.NewGuid() };

            _orderRepoMock
                .Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), query.Id))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var query = new GetOrderByIdQuery { Id = Guid.NewGuid() };

            _orderRepoMock
                .Setup(repo => repo.GetOrderById(It.IsAny<CancellationToken>(), query.Id))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database connection failed");
        }
    }
}
