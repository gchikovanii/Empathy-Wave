using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Orders;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class UdpateOrderCommandHandler(IOrderRepository repo, IUnitOfWork unit) : IRequestHandler<UpdateOrderCommand, Result>
    {
        private readonly IOrderRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _repo.GetOrderById(cancellationToken, request.Id).ConfigureAwait(false);
            if (order.UserId != request.UserId)
                return Result.Failure(OrderErrors.InaccessibleOrder);
            if (order == null)
                return Result.Failure(OrderErrors.OrderNotExists);
            order.Status = request.Status;
            order.UpdatedAt = DateTimeOffset.UtcNow;    
            _repo.UpdateOrder(order);
            var result = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (result == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }
    }
}
