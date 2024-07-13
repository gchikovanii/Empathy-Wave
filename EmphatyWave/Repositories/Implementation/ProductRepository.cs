using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class ProductRepository : IProductRepository
    {
        private readonly IBaseRepository<Product> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductRepository(IBaseRepository<Product> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ICollection<Product>> GetProducts(CancellationToken token, int pageNumber, int pageSize)
        {
           return await _repository.GetPaginatedData(token,pageNumber, pageSize).ConfigureAwait(false);
        }

        public async Task<Product> GetProductById(CancellationToken token, Guid productId)
        {
            var product = await _repository.GetDataById(token, productId).ConfigureAwait(false);
            return product ?? new Product { };
        }

        public async Task<bool> CreateProductAsync(CancellationToken token, Product product)
        {
            await _repository.CreateData(token, product).ConfigureAwait(false);
            return await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
        }
        public async Task<bool> UpdateProduct(CancellationToken token, Product product)
        {
            _repository.UpdateData(product);
            return await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
        }
        public async Task DeleteProduct(CancellationToken token, Guid productId)
        {
            var product = await _repository.GetDataById(token, productId).ConfigureAwait(false);
            if(product != null)
            {
                _repository.DeleteData(product);
                await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
            }
        }
    }
}
