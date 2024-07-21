using EmphatyWave.Application.Queries.Orders.DTOs;
using EmphatyWave.Application.Queries.Orders;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using FluentAssertions;
using Mapster;
using Moq;

namespace EmphatyWave.Application.Tests.Orders.Queries
{
    public class GetOrdersForUserQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly GetOrdersForUserQueryHandler _handler;

        public GetOrdersForUserQueryHandlerTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _handler = new GetOrdersForUserQueryHandler(_orderRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnOrderDtos_WhenOrdersExistForUser()
        {
            // Arrange
            var query = new GetOrdersForUserQuery
            {
                PageNumber = 1,
                PageSize = 10,
                UserId = "user1"
            };

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
                Status = Status.Shipping,
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
                UserId = "user1",
                StripeToken = "token456",
                User = new User()
            }
        };

            var expectedOrderDtos = orderList.Adapt<List<OrderDto>>();

            _orderRepoMock
                .Setup(repo => repo.GetOrdersForUser(It.IsAny<CancellationToken>(), query.PageNumber, query.PageSize, query.UserId))
                .ReturnsAsync(orderList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedOrderDtos);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoOrdersExistForUser()
        {
            // Arrange
            var query = new GetOrdersForUserQuery
            {
                PageNumber = 1,
                PageSize = 10,
                UserId = "user1"
            };

            _orderRepoMock
                .Setup(repo => repo.GetOrdersForUser(It.IsAny<CancellationToken>(), query.PageNumber, query.PageSize, query.UserId))
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
            var query = new GetOrdersForUserQuery
            {
                PageNumber = 1,
                PageSize = 10,
                UserId = "user1"
            };

            _orderRepoMock
                .Setup(repo => repo.GetOrdersForUser(It.IsAny<CancellationToken>(), query.PageNumber, query.PageSize, query.UserId))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database connection failed");
        }
    }
}
