using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class OrderItemRepository(IBaseRepository<OrderItem> repository) : IOrderItemRepository
    {
        private readonly IBaseRepository<OrderItem> _repository = repository;

        public async Task<ICollection<OrderItem>> GetOrderItems(CancellationToken token, Guid orderId)
        {
            return await _repository.GetQuery(i => i.OrderId == orderId).ToListAsync(token).ConfigureAwait(false);
        }

        public async Task<OrderItem> GetOrderItemsById(CancellationToken token, Guid id)
        {
            return await _repository.GetDataById(token, id).ConfigureAwait(false);
        }
        public async Task CreateOrderItemsAsync(CancellationToken token, OrderItem order)
        {
            await _repository.CreateData(token, order).ConfigureAwait(false);   
        }
        public async Task AddOrderItems(CancellationToken token, List<OrderItem> orderItems)
        {
            await _repository.AddRange(token,orderItems).ConfigureAwait(false);
        }
        public void UpdateOrderItem(OrderItem order)
        {
            _repository.UpdateData(order);
        }

        public async Task DeleteOrderItem(CancellationToken token, Guid id)
        {
            var orderItem = await _repository.GetDataById(token, id).ConfigureAwait(false);
            if (orderItem != null)
            {
                _repository.DeleteData(orderItem);
            }
        }
       
    }
}
