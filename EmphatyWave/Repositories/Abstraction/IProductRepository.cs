using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IProductRepository
    {
        Task<ICollection<Product>> GetProducts(CancellationToken token, int pageNumber, int pageSize);
        Task<Product> GetProductById(CancellationToken token, Guid productId);
        Task<Product> GetProductByName(CancellationToken token, string productName);
        Task CreateProductAsync(CancellationToken token, Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(Product product);
    }
}
