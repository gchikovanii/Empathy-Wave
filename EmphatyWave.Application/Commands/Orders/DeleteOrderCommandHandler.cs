using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Orders;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class DeleteOrderCommandHandler(IOrderRepository repo, IOrderItemRepository orderItemrepo,
        IUnitOfWork unit) : IRequestHandler<DeleteOrderCommand, Result>
    {
        private readonly IOrderRepository _repo = repo;
        private readonly IOrderItemRepository _orderItemrepo = orderItemrepo;
        private readonly IUnitOfWork _unit = unit;
        public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _repo.GetOrderById(cancellationToken, request.Id).ConfigureAwait(false);
            if (order.UserId != request.UserId)
                return Result.Failure(OrderErrors.InaccessibleOrder);
            if (order == null)
                return Result.Failure(OrderErrors.OrderNotExists);
            _repo.DeleteOrder(order);
            var orderItems = await _orderItemrepo.GetOrderItems(cancellationToken,request.Id).ConfigureAwait(false);
            if(orderItems == null)
                return Result.Failure(OrderErrors.OrderItemsNotExist);
            foreach (var item in orderItems)
            {
                await _orderItemrepo.DeleteOrderItem(cancellationToken, item.Id).ConfigureAwait(false);
            }
            var result = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (result == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }
    }
}
