using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class OrderRepository(IBaseRepository<Order> repository) : IOrderRepository
    {
        private readonly IBaseRepository<Order> _repository = repository;

        public async Task<Order> GetOrderById(CancellationToken token, Guid id)
        {
            return await _repository.GetDataById(token, id).ConfigureAwait(false);
        }

        public async Task<ICollection<Order>> GetOrders(CancellationToken token, int pageNumber, int pageSize)
        {
            return await _repository.GetPaginatedData(token,pageNumber, pageSize).ConfigureAwait(false);
        }
        public async Task<ICollection<Order>> GetOrdersForUser(CancellationToken token, int pageNumber, int pageSize, string userId)
        {
            var result = await _repository.GetQuery(i => i.UserId == userId).AsNoTracking().Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(token).ConfigureAwait(false);
            return result;
        }

        public async Task CreateOrderAsync(CancellationToken token, Order order)
        {
            await _repository.CreateData(token, order).ConfigureAwait(false);
        }
        public void UpdateOrder(Order order)
        {
            _repository.UpdateData(order);
        }
        public async Task DeleteOrder(CancellationToken token, Guid id)
        {
            var order = await _repository.GetDataById(token,id);
            if (order != null)
            {
                _repository.DeleteData(order);
            }
            else
                throw new Exception("Order Not found!");
        }
    }
}
