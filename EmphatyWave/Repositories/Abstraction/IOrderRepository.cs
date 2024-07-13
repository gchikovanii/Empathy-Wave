using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IOrderRepository
    {
        Task<ICollection<Order>> GetOrders(CancellationToken token, int pageNumber, int pageSize);
        Task<Order> GetOrderById(CancellationToken token, Guid id);
        Task<bool> CreateOrderAsync(CancellationToken token, Order order);
        Task<bool> UpdateOrder(CancellationToken token, Order order);
        Task DeleteOrder(CancellationToken token, Guid id);
    }
}
