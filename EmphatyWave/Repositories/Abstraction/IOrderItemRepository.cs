using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IOrderItemRepository
    {
        Task<ICollection<OrderItem>> GetOrderItems(CancellationToken token, Guid orderId);
        Task<OrderItem> GetOrderItemsById(CancellationToken token, Guid id);
        Task AddOrderItems(CancellationToken token, List<OrderItem> orderItems);
        Task CreateOrderItemsAsync(CancellationToken token, OrderItem order);
        void UpdateOrderItem(OrderItem order);
        Task DeleteOrderItem(CancellationToken token, Guid id);
    }
}
