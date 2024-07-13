using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IOrderItemRepository
    {
        Task<ICollection<OrderItem>> GetOrderItems(CancellationToken token, Guid orderId);
        Task<OrderItem> GetOrderItemsById(CancellationToken token, Guid id);
        Task<bool> CreateOrderItemsAsync(CancellationToken token, OrderItem order);
        Task<bool> UpdateOrderItem(CancellationToken token, OrderItem order);
        Task DeleteOrderItem(CancellationToken token, Guid id);
    }
}
