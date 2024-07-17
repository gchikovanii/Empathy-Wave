using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class ProductRepository(IBaseRepository<Product> repository) : IProductRepository
    {
        private readonly IBaseRepository<Product> _repository = repository;

        public async Task<ICollection<Product>> GetProducts(CancellationToken token, int pageNumber, int pageSize)
        {
           return await _repository.GetPaginatedData(token,pageNumber, pageSize).ConfigureAwait(false);
        }

        public async Task<Product> GetProductById(CancellationToken token, Guid productId)
        {
            var product = await _repository.GetDataById(token, productId).ConfigureAwait(false);
            return product ?? new Product { };
        }

        public async Task CreateProductAsync(CancellationToken token, Product product)
        {
            product.Id = Guid.NewGuid();
            await _repository.CreateData(token, product).ConfigureAwait(false);
        }
        public void UpdateProduct(Product product)
        {
            _repository.UpdateData(product);
        }
        public async Task DeleteProduct(CancellationToken token, Guid productId)
        {
            var product = await _repository.GetDataById(token, productId).ConfigureAwait(false);
            if(product != null)
            {
                _repository.DeleteData(product);
            }
        }
    }
}
