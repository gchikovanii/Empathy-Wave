using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class DeleteOrderCommandHandler(IOrderRepository repo, IOrderItemRepository orderItemrepo,
        IUnitOfWork unit) : IRequestHandler<DeleteOrderCommand, bool>
    {
        private readonly IOrderRepository _repo = repo;
        private readonly IOrderItemRepository _orderItemrepo = orderItemrepo;
        private readonly IUnitOfWork _unit = unit;
        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            await _repo.DeleteOrder(cancellationToken,request.Id).ConfigureAwait(false);
            var orderItems = await _orderItemrepo.GetOrderItems(cancellationToken,request.Id).ConfigureAwait(false);
            foreach (var item in orderItems)
            {
                await _orderItemrepo.DeleteOrderItem(cancellationToken, item.Id).ConfigureAwait(false);
            }
            return await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
