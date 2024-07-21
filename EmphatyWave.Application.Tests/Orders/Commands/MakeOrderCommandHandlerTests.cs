using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Commands.Orders.DTOs;
using EmphatyWave.Application.Services.PromoCodes.Abstraction;
using EmphatyWave.Application.Services.Stripe.Abstraction;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Products;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.PromoCodeErrors;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace EmphatyWave.Application.Tests.Orders.Commands
{
    public class MakeOrderCommandHandlerTests
    {
        private readonly Mock<IValidator<MakeOrderCommand>> _mockValidator;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPromoCodeService> _mockPromoCode;
        private readonly MakeOrderCommandHandler _handler;

        public MakeOrderCommandHandlerTests()
        {
            _mockValidator = new Mock<IValidator<MakeOrderCommand>>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockOrderItemRepository = new Mock<IOrderItemRepository>();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockPromoCode = new Mock<IPromoCodeService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _handler = new MakeOrderCommandHandler(
                _mockOrderRepository.Object,
                _mockProductRepo.Object,
                _mockValidator.Object,
                _mockOrderItemRepository.Object,
                _mockUnitOfWork.Object,
                _mockPaymentService.Object,
                _mockPromoCode.Object
            );
        }
        [Fact]
        public async Task MakeOrder_ShouldReturnFailure_WhenValidationFails()
        {
            // Arrange
            var command = new MakeOrderCommand();
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("PropertyName", "ErrorMessage")
            });
            _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be("ValidationError");
        }

    }
}
