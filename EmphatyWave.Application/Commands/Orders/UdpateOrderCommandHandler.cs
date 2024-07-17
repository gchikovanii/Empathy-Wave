using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class UdpateOrderCommandHandler(IOrderRepository repo, IUnitOfWork unit) : IRequestHandler<UpdateOrderCommand, bool>
    {
        private readonly IOrderRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _repo.GetOrderById(cancellationToken, request.Id).ConfigureAwait(false);
            if (order == null)
                throw new Exception("Order not found");
            order.Status = request.Status;
            order.UpdatedAt = DateTimeOffset.UtcNow;    
            _repo.UpdateOrder(order);
            return await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
