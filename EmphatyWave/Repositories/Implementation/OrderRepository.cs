using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IBaseRepository<Order> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderRepository(IBaseRepository<Order> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Order> GetOrderById(CancellationToken token, Guid id)
        {
            return await _repository.GetDataById(token, id).ConfigureAwait(false);
        }

        public async Task<ICollection<Order>> GetOrders(CancellationToken token, int pageNumber, int pageSize)
        {
            return await _repository.GetPaginatedData(token,pageNumber, pageSize).ConfigureAwait(false);
        }

        public async Task<bool> CreateOrderAsync(CancellationToken token, Order order)
        {
            await _repository.CreateData(token, order).ConfigureAwait(false);
            return await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task<bool> UpdateOrder(CancellationToken token, Order order)
        {
            _repository.UpdateData(order);
            return await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
        }
        public async Task DeleteOrder(CancellationToken token, Guid id)
        {
            var product = await _repository.GetDataById(token,id);
            if (product != null)
            {
                _repository.DeleteData(product);
                await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
            }
        }
    }
}
