using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IProductImageRepository
    {
        Task<ICollection<ProductImage>> GetImages(CancellationToken token, Guid productId);
        Task<bool> CreateOrderImageAsync(CancellationToken token, ProductImage productImage);
        Task DeleteOrderImage(CancellationToken token, Guid id);
    }
}
