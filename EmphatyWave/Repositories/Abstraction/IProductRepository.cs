using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IProductRepository
    {
        Task<ICollection<Product>> GetProducts(CancellationToken token, int pageNumber, int pageSize);
        Task<Product> GetProductById(CancellationToken token, Guid productId);
        Task<bool> CreateProductAsync(CancellationToken token, Product product);
        Task<bool> UpdateProduct(CancellationToken token, Product product);
        Task DeleteProduct(CancellationToken token, Guid productId);
    }
}
