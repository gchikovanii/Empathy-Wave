using EmphatyWave.Application.Services.Stripe.Abstraction;
using EmphatyWave.Domain;
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
        : IRequestHandler<MakeOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository = orderRepo;
        private readonly IPaymentService _paymentService = paymentService;
        private readonly IOrderItemRepository _orderItemRepository = orderItemRepo;
        private readonly IProductRepository _productRepo = productRepo;
        private readonly IValidator<MakeOrderCommand> _validator = validator;
        private readonly IUnitOfWork _unit = unit;

        public async Task<bool> Handle(MakeOrderCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();
            using var transaction = await _unit.BeginTransaction(IsolationLevel.RepeatableRead, cancellationToken).ConfigureAwait(false);

            try
            {
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
                        throw new Exception($"One of the products was not found! {item.ProductId} - tried to search with this id");
                    if (product.Quantity < item.Quantity)
                        throw new Exception($"IN Stock it's not enough products with id: {product.Id} Doesn't exists any more");
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

                await _orderRepository.CreateOrderAsync(cancellationToken, order);
                await _orderItemRepository.AddOrderItems(cancellationToken, orderItems).ConfigureAwait(false);

                var charge = await _paymentService.ProcessPayment(totalAmount, "gel", "Payment of Order",
                    request.PaymentDetails.StripeToken).ConfigureAwait(false);
                if (charge.Status != "succeeded")
                {
                    throw new Exception("Payment failed: " + charge.FailureMessage);
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
                        throw new Exception("Payment failed: " + charge.FailureMessage);
                }
                _orderRepository.UpdateOrder(order);
                await _orderItemRepository.AddOrderItems(cancellationToken, orderItems).ConfigureAwait(false);
                await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception(ex.Message);
            }
        }
    }
}
