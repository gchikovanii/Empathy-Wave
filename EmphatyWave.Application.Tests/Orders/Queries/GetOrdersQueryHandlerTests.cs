using EmphatyWave.Application.Queries.Orders.DTOs;
using EmphatyWave.Application.Queries.Orders;
using EmphatyWave.Persistence.Repositories.Abstraction;
using FluentAssertions;
using Mapster;
using Moq;
using EmphatyWave.Domain;

namespace EmphatyWave.Application.Tests.Orders.Queries
{
    public class GetOrdersQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly GetOrdersQueryHandler _handler;

        public GetOrdersQueryHandlerTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _handler = new GetOrdersQueryHandler(_orderRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnOrderDtos_WhenOrdersExist()
        {
            // Arrange
            var query = new GetOrdersQuery { PageNumber = 1, PageSize = 10 };
            var orderList = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                OrderItems = new List<OrderItem>(),
                TotalAmount = 100,
                ShippingDetails = new ShippingDetail(),
                Status = Status.PaymentPending,
                UserId = "user1",
                StripeToken = "token123",
                User = new User()
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                OrderItems = new List<OrderItem>(),
                TotalAmount = 200,
                ShippingDetails = new ShippingDetail(),
                Status = Status.Delivered,
                UserId = "user2",
                StripeToken = "token456",
                User = new User()
            }
        };

            var expectedOrderDtos = orderList.Adapt<List<OrderDto>>();

            _orderRepoMock
                .Setup(repo => repo.GetOrders(It.IsAny<CancellationToken>(), query.PageNumber, query.PageSize))
                .ReturnsAsync(orderList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedOrderDtos);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var query = new GetOrdersQuery { PageNumber = 1, PageSize = 10 };

            _orderRepoMock
                .Setup(repo => repo.GetOrders(It.IsAny<CancellationToken>(), query.PageNumber, query.PageSize))
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var query = new GetOrdersQuery { PageNumber = 1, PageSize = 10 };

            _orderRepoMock
                .Setup(repo => repo.GetOrders(It.IsAny<CancellationToken>(), query.PageNumber, query.PageSize))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database connection failed");
        }
    }
}
