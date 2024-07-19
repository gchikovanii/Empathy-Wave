using EmphatyWave.Application.Services.Stripe.Abstraction;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Products;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;
using System.Data;

namespace EmphatyWave.Application.Commands.Orders
{
    public class MakeOrderCommandHandler(IOrderRepository orderRepo, IProductRepository productRepo, IValidator<MakeOrderCommand> validator,
        IOrderItemRepository orderItemRepo, IUnitOfWork unit, IPaymentService paymentService)
        : IRequestHandler<MakeOrderCommand, Result>
    {
        private readonly IOrderRepository _orderRepository = orderRepo;
        private readonly IPaymentService _paymentService = paymentService;
        private readonly IOrderItemRepository _orderItemRepository = orderItemRepo;
        private readonly IProductRepository _productRepo = productRepo;
        private readonly IValidator<MakeOrderCommand> _validator = validator;
        private readonly IUnitOfWork _unit = unit;

        public async Task<Result> Handle(MakeOrderCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

            if (!result.IsValid)
            {
                var errorMessages = result.Errors.Select(e => e.ErrorMessage);
                string errorMessage = string.Join("; ", errorMessages);
                Error error = new ("ValidationError", errorMessage);
                return Result.Failure(error);
            }
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();
            using var transaction = await _unit.BeginTransaction(System.Transactions.IsolationLevel.RepeatableRead, cancellationToken).ConfigureAwait(false);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Status = Status.PaymentPending,
                OrderItems = request.OrderItems.Adapt<List<OrderItem>>(),
                TotalAmount = totalAmount,
                UserId = request.UserId,
                ShippingDetails = new ShippingDetail
                {
                    Address = request.ShippingDetails.Address,
                    CountryCode = request.ShippingDetails.CountryCode,
                    PhoneNumber = request.ShippingDetails.PhoneNumber,
                    ZipCode = request.ShippingDetails.ZipCode
                }
            };
            foreach (var item in request.OrderItems)
            {
                var product = await _productRepo.GetProductById(cancellationToken, item.ProductId).ConfigureAwait(false);
                if (product == null)
                    return Result.Failure(ProductErrors.ProductNotFound);
                if (product.Quantity < item.Quantity)
                    return Result.Failure(ProductErrors.OutOfStock);
                totalAmount += product.Price * item.Quantity;
                item.Price = product.Price;
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Price = product.Price,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId
                };
                orderItems.Add(orderItem);
                product.Quantity -= item.Quantity;
                _productRepo.UpdateProduct(product);
            }

            try
            {
                await _orderRepository.CreateOrderAsync(cancellationToken, order);
                await _orderItemRepository.AddOrderItems(cancellationToken, orderItems).ConfigureAwait(false);

                var charge = await _paymentService.ProcessPayment(totalAmount, "gel", "Payment of Order",
                    request.PaymentDetails.StripeToken).ConfigureAwait(false);
                Error failureError = new("FailureError", charge.FailureMessage);
                if (charge.Status != "succeeded")
                {
                    transaction.Rollback();
                    return Result.Failure(failureError);
                }

                switch (charge.Status)
                {
                    case "succeeded":
                        order.Status = Status.PaymentSucceeded;
                        break;
                    case "pending":
                        order.Status = Status.PaymentPending;
                        order.StripeToken = request.PaymentDetails.StripeToken;
                        break;
                    default:
                        return Result.Failure(failureError);
                }
                _orderRepository.UpdateOrder(order);
                var saves = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                if (saves == false)
                {
                    transaction.Rollback();
                    return Result.Failure(UnitError.CantSaveChanges);
                }
                transaction.Commit();
                return Result.Success();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Result.Failure(new Error("Unexpected Exception", ex.Message));
            }
        }
    }
}
