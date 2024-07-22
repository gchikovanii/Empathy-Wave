using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Commands.Orders.DTOs;
using EmphatyWave.Application.Commands.Orders.Models;
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
using System.Data;

namespace EmphatyWave.Application.Tests.Orders.Commands
{
    public class MakeOrderCommandHandlerTests
    {
        private readonly Mock<IValidator<MakeOrderCommand>> _validatorMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepoMock;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly Mock<IPromoCodeService> _promoCodeServiceMock;
        private readonly MakeOrderCommandHandler _handler;

        public MakeOrderCommandHandlerTests()
        {
            _validatorMock = new Mock<IValidator<MakeOrderCommand>>();
            _productRepoMock = new Mock<IProductRepository>();
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderItemRepoMock = new Mock<IOrderItemRepository>();
            _paymentServiceMock = new Mock<IPaymentService>();
            _promoCodeServiceMock = new Mock<IPromoCodeService>();
            _unitMock = new Mock<IUnitOfWork>();
            _handler = new MakeOrderCommandHandler(
                _orderRepoMock.Object,
                _productRepoMock.Object,
                _validatorMock.Object,
                _orderItemRepoMock.Object,
                _unitMock.Object,
                _paymentServiceMock.Object,
                _promoCodeServiceMock.Object
            );
        }
        [Fact]
        public async Task MakeOrder_Should_ReturnFailure_WhenInvalidDataInputed()
        {
            // Arrange
            var command = new MakeOrderCommand();
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("PropertyName", "ErrorMessage")
            });
            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be("ValidationError");
        }

        [Fact]
        public async Task MakeOrder_Should_ReturnFailure_WhenProductNotFound()
        {
            // Arrange
            var command = new MakeOrderCommand
            {
                UserId = Guid.NewGuid().ToString(),
                ShippingDetails = new ShippingDetail
                {
                    Address = "123 Test St",
                    CountryCode = "US",
                    PhoneNumber = "1234567890",
                    ZipCode = "12345"
                },
                OrderItems = new List<OrderItemDto>
                {
                        new OrderItemDto
                        {
                            ProductId = Guid.NewGuid(),
                            Quantity = 1
                        }
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
                    _productRepoMock.Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.OrderItems[0].ProductId))
                .ReturnsAsync((Product)null);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue(); 
            result.Error.code.Should().Be(ProductErrors.ProductNotFound.code);
            result.Error.descripton.Should().Be(ProductErrors.ProductNotFound.descripton);
        }

        [Fact]
        public async Task MakeOrder_Should_ReturnFailure_WhenProductIsOutOfStock()
        {
            // Arrange
            var command = new MakeOrderCommand
            {
                UserId = Guid.NewGuid().ToString(),
                ShippingDetails = new ShippingDetail
                {
                    Address = "123 Test St",
                    CountryCode = "US",
                    PhoneNumber = "1234567890",
                    ZipCode = "12345"
                },
                OrderItems = new List<OrderItemDto>
                {
                        new OrderItemDto
                        {
                            ProductId = Guid.NewGuid(),
                            Quantity = 5
                        }
                }
            };
            var existingProduct = new Product
            {
                Id = command.OrderItems[0].ProductId,
                Quantity = 2
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _productRepoMock.Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.OrderItems[0].ProductId))
                .ReturnsAsync(existingProduct);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be(ProductErrors.OutOfStock.code);
            result.Error.descripton.Should().Be(ProductErrors.OutOfStock.descripton);
        }

        [Fact]
        public async Task MakeOrder_Should_ReturnFailure_WhenProductInvalidPromoCode()
        {
            // Arrange
            var command = new MakeOrderCommand
            {
                UserId = Guid.NewGuid().ToString(),
                ShippingDetails = new ShippingDetail
                {
                    Address = "123 Test St",
                    CountryCode = "US",
                    PhoneNumber = "1234567890",
                    ZipCode = "12345"
                },
                OrderItems = new List<OrderItemDto>
                {
                        new OrderItemDto
                        {
                            ProductId = Guid.NewGuid(),
                            Quantity = 5
                        }
                },
                PromoCodeName = "PromoInValid"
            };
            var existingProduct = new Product
            {
                Id = command.OrderItems[0].ProductId,
                Quantity = 25
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _productRepoMock.Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.OrderItems[0].ProductId))
                .ReturnsAsync(existingProduct);
            _promoCodeServiceMock.Setup(service => service.GetPromoCodeByPromoName(It.IsAny<CancellationToken>(), command.PromoCodeName))
                .ReturnsAsync((PromoCode)null);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be(PromoCodeErrors.PromoCodeError.code);
        }
        [Fact]
        public async Task MakeOrder_Should_ReturnFailure_WhenProductInvalidUserPromoCode()
        {
            // Arrange
            var command = new MakeOrderCommand
            {
                UserId = Guid.NewGuid().ToString(),
                ShippingDetails = new ShippingDetail
                {
                    Address = "123 Test St",
                    CountryCode = "US",
                    PhoneNumber = "1234567890",
                    ZipCode = "12345"
                },
                OrderItems = new List<OrderItemDto>
                {
                        new OrderItemDto
                        {
                            ProductId = Guid.NewGuid(),
                            Quantity = 5
                        }
                },
                PromoCodeName = "PromoInValid"
            };
            var existingProduct = new Product
            {
                Id = command.OrderItems[0].ProductId,
                Quantity = 25
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _productRepoMock.Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.OrderItems[0].ProductId))
                .ReturnsAsync(existingProduct);
            _promoCodeServiceMock.Setup(service => service.GetPromoCode(It.IsAny<CancellationToken>(), command.UserId, Guid.NewGuid()))
                .ReturnsAsync((UserPromoCode)null);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be(PromoCodeErrors.PromoCodeError.code);
        }

        [Fact]
        public async Task MakeOrder_Should_ReturnFailure_WhenProductInfoIsInvalid()
        {
            // Arrange
            var command = new MakeOrderCommand
            {
                UserId = Guid.NewGuid().ToString(),
                ShippingDetails = new ShippingDetail
                {
                    Address = "123 Test St",
                    CountryCode = "US",
                    PhoneNumber = "1234567890",
                    ZipCode = "12345"
                },
                OrderItems = new List<OrderItemDto>
                {
                        new OrderItemDto
                        {
                            ProductId = Guid.NewGuid(),
                            Quantity = 5
                        }
                },
                PromoCodeName = "PromoInValid"
            };
            var existingProduct = new Product
            {
                Id = command.OrderItems[0].ProductId,
                Quantity = 25
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _productRepoMock.Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.OrderItems[0].ProductId))
                .ReturnsAsync(existingProduct);
            _promoCodeServiceMock.Setup(service => service.GetPromoCodeInfo(It.IsAny<CancellationToken>(), Guid.NewGuid()))
                .ReturnsAsync((PromoCode)null);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be(PromoCodeErrors.PromoCodeError.code);
        }

        [Fact]
        public async Task MakeOrder_Should_ReturnFailure_WhenChargingFailed()
        {
            // Arrange
            var command = new MakeOrderCommand
            {
                UserId = Guid.NewGuid().ToString(),
                ShippingDetails = new ShippingDetail
                {
                    Address = "123 Test St",
                    CountryCode = "US",
                    PhoneNumber = "1234567890",
                    ZipCode = "12345"
                },
                OrderItems = new List<OrderItemDto>
        {
            new OrderItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 5
            }
        },
                PromoCodeName = "ValidPromoCode",
                PaymentDetails = new PaymentDetails 
                {
                    StripeToken = "StripeTokenWhichIsForTestingOnlyyy"
                }
            };

            var existingProduct = new Product
            {
                Id = command.OrderItems[0].ProductId,
                Quantity = 25
            };

            var promoCode = new PromoCode
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                DiscountPercentage = 10,
                Name = command.PromoCodeName
            };

            var userPromoCode = new UserPromoCode
            {
                UserId = command.UserId,
                PromoCodeId = promoCode.Id
            };

            // Mock setup
            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _productRepoMock.Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.OrderItems[0].ProductId))
                .ReturnsAsync(existingProduct);

            _promoCodeServiceMock.Setup(service => service.GetPromoCodeByPromoName(It.IsAny<CancellationToken>(), command.PromoCodeName))
                .ReturnsAsync(promoCode);
            _promoCodeServiceMock.Setup(service => service.GetPromoCodeInfo(It.IsAny<CancellationToken>(), promoCode.Id))
                .ReturnsAsync(promoCode);
            _promoCodeServiceMock.Setup(service => service.GetPromoCode(It.IsAny<CancellationToken>(), command.UserId, promoCode.Id))
                .ReturnsAsync(userPromoCode);
            _promoCodeServiceMock.Setup(service => service.RedeemPromoCodeAsync(It.IsAny<CancellationToken>(), command.UserId, promoCode.Id))
                .ReturnsAsync(userPromoCode);

            var charge = new Stripe.Charge
            {
                Status = "failed",
                FailureMessage = "Payment failed due to insufficient funds"
            };
            _paymentServiceMock.Setup(i => i.ProcessPayment(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), command.PaymentDetails.StripeToken))
                .ReturnsAsync(charge);

            _unitMock.Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IDbTransaction>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.code.Should().Be("FailureError");
            result.Error.descripton.Should().Be("Payment failed due to insufficient funds");
        }


        [Fact]
        public async Task MakeOrder_Should_ReturnFailure_WhenSaveChangesFailed()
        {
            // Arrange
            var command = new MakeOrderCommand
            {
                UserId = Guid.NewGuid().ToString(),
                ShippingDetails = new ShippingDetail
                {
                    Address = "123 Test St",
                    CountryCode = "US",
                    PhoneNumber = "1234567890",
                    ZipCode = "12345"
                },
                OrderItems = new List<OrderItemDto>
        {
            new OrderItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 5
            }
        },
                PromoCodeName = "ValidPromoCode",
                PaymentDetails = new PaymentDetails
                {
                    StripeToken = "StripeTokenWhichIsForTestingOnlyyy"
                }
            };

            var existingProduct = new Product
            {
                Id = command.OrderItems[0].ProductId,
                Quantity = 25
            };

            var promoCode = new PromoCode
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                DiscountPercentage = 10,
                Name = command.PromoCodeName
            };

            var userPromoCode = new UserPromoCode
            {
                UserId = command.UserId,
                PromoCodeId = promoCode.Id
            };

            // Mock setup
            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _productRepoMock.Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.OrderItems[0].ProductId))
                .ReturnsAsync(existingProduct);

            _promoCodeServiceMock.Setup(service => service.GetPromoCodeByPromoName(It.IsAny<CancellationToken>(), command.PromoCodeName))
                .ReturnsAsync(promoCode);
            _promoCodeServiceMock.Setup(service => service.GetPromoCodeInfo(It.IsAny<CancellationToken>(), promoCode.Id))
                .ReturnsAsync(promoCode);
            _promoCodeServiceMock.Setup(service => service.GetPromoCode(It.IsAny<CancellationToken>(), command.UserId, promoCode.Id))
                .ReturnsAsync(userPromoCode);
            _promoCodeServiceMock.Setup(service => service.RedeemPromoCodeAsync(It.IsAny<CancellationToken>(), command.UserId, promoCode.Id))
                .ReturnsAsync(userPromoCode);

            var charge = new Stripe.Charge
            {
                Status = "succeeded"
            };
            _paymentServiceMock.Setup(i => i.ProcessPayment(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), command.PaymentDetails.StripeToken))
                .ReturnsAsync(charge);

            _unitMock.Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IDbTransaction>());
            _unitMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UnitError.CantSaveChanges);
        }

        [Fact]
        public async Task MakeOrder_Should_ReturnSuccess()
        {
            // Arrange
            var command = new MakeOrderCommand
            {
                UserId = Guid.NewGuid().ToString(),
                ShippingDetails = new ShippingDetail
                {
                    Address = "123 Test St",
                    CountryCode = "US",
                    PhoneNumber = "1234567890",
                    ZipCode = "12345"
                },
                OrderItems = new List<OrderItemDto>
        {
            new OrderItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 5
            }
        },
                PromoCodeName = "ValidPromoCode",
                PaymentDetails = new PaymentDetails
                {
                    StripeToken = "StripeTokenWhichIsForTestingOnlyyy"
                }
            };

            var existingProduct = new Product
            {
                Id = command.OrderItems[0].ProductId,
                Quantity = 25
            };

            var promoCode = new PromoCode
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                DiscountPercentage = 10,
                Name = command.PromoCodeName
            };

            var userPromoCode = new UserPromoCode
            {
                UserId = command.UserId,
                PromoCodeId = promoCode.Id
            };

            // Mock setup
            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _productRepoMock.Setup(repo => repo.GetProductById(It.IsAny<CancellationToken>(), command.OrderItems[0].ProductId))
                .ReturnsAsync(existingProduct);

            _promoCodeServiceMock.Setup(service => service.GetPromoCodeByPromoName(It.IsAny<CancellationToken>(), command.PromoCodeName))
                .ReturnsAsync(promoCode);
            _promoCodeServiceMock.Setup(service => service.GetPromoCodeInfo(It.IsAny<CancellationToken>(), promoCode.Id))
                .ReturnsAsync(promoCode);
            _promoCodeServiceMock.Setup(service => service.GetPromoCode(It.IsAny<CancellationToken>(), command.UserId, promoCode.Id))
                .ReturnsAsync(userPromoCode);
            _promoCodeServiceMock.Setup(service => service.RedeemPromoCodeAsync(It.IsAny<CancellationToken>(), command.UserId, promoCode.Id))
                .ReturnsAsync(userPromoCode);

            var charge = new Stripe.Charge
            {
                Status = "succeeded"
            };
            _paymentServiceMock.Setup(i => i.ProcessPayment(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), command.PaymentDetails.StripeToken))
                .ReturnsAsync(charge);

            _unitMock.Setup(u => u.BeginTransaction(It.IsAny<System.Transactions.IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IDbTransaction>());
            _unitMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}
