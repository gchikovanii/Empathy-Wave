using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IProductImageRepository
    {
        Task<ICollection<ProductImage>> GetImages(CancellationToken token, Guid productId);
        Task<bool> CreateProductImageAsync(CancellationToken token, ProductImage productImage);
        Task DeleteProductImage(CancellationToken token, Guid id);
    }
}
