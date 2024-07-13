using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly IBaseRepository<OrderItem> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderItemRepository(IBaseRepository<OrderItem> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ICollection<OrderItem>> GetOrderItems(CancellationToken token, Guid orderId)
        {
            return await _repository.GetQuery(i => i.OrderId == orderId).ToListAsync(token).ConfigureAwait(false);
        }

        public async Task<OrderItem> GetOrderItemsById(CancellationToken token, Guid id)
        {
            return await _repository.GetDataById(token, id).ConfigureAwait(false);
        }
        public async Task<bool> CreateOrderItemsAsync(CancellationToken token, OrderItem order)
        {
            await _repository.CreateData(token, order).ConfigureAwait(false);   
            return await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
        }

        public async Task<bool> UpdateOrderItem(CancellationToken token, OrderItem order)
        {
            _repository.UpdateData(order);
            return await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
        }

        public async Task DeleteOrderItem(CancellationToken token, Guid id)
        {
            var orderItem = await _repository.GetDataById(token, id).ConfigureAwait(false);
            if (orderItem != null)
            {
                _repository.DeleteData(orderItem);
                await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
            }
        }
       
    }
}
