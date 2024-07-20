using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class ProductRepository(IBaseRepository<Product> repository) : IProductRepository
    {
        private readonly IBaseRepository<Product> _repository = repository;

        public async Task<ICollection<Product>> GetProducts(CancellationToken token, int pageNumber, int pageSize)
        {
            return await _repository.GetPaginatedData(token, pageNumber, pageSize).ConfigureAwait(false);
        }
        public async Task<Product> GetProductByName(CancellationToken token, string productName)
        {
            return await _repository.GetQuery(i => i.Name == productName).FirstOrDefaultAsync(token).ConfigureAwait(false) ?? new Product { };
        }
        public async Task<Product> GetProductById(CancellationToken token, Guid productId)
        {
            var product = await _repository.GetDataById(token, productId).ConfigureAwait(false);
            return product ?? new Product { };
        }

        public async Task CreateProductAsync(CancellationToken token, Product product)
        {
            await _repository.CreateData(token, product).ConfigureAwait(false);
        }
        public void UpdateProduct(Product product)
        {
            _repository.UpdateData(product);
        }
        public void DeleteProduct(Product product)
        {
            _repository.DeleteData(product);
        }
    }
}
