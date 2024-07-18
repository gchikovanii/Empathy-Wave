using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IOrderRepository
    {
        Task<ICollection<Order>> GetOrders(CancellationToken token, int pageNumber, int pageSize);
        Task<ICollection<Order>> GetOrdersForUser(CancellationToken token, int pageNumber, int pageSize, string userId);
        Task<ICollection<Order>> GetPendingOrdersAsync(CancellationToken token);
        Task<Order> GetOrderById(CancellationToken token, Guid id);
        Task CreateOrderAsync(CancellationToken token, Order order);
        void UpdateOrder(Order order);
        Task DeleteOrder(CancellationToken token, Guid id);
    }
}
