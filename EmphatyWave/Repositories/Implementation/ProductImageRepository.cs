using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class ProductImageRepository(IBaseRepository<ProductImage> repository) : IProductImageRepository
    {
        private readonly IBaseRepository<ProductImage> _repository =repository;

        public async Task<ICollection<ProductImage>> GetImages(CancellationToken token, Guid productId)
        {
            return await _repository.GetQuery(i => i.ProductId == productId).ToListAsync(token).ConfigureAwait(false);
        }
        public async Task CreateProductImageAsync(CancellationToken token,ProductImage productImage)
        {
            await _repository.CreateData(token, productImage).ConfigureAwait(false);
        }

        public async Task DeleteProductImage(CancellationToken token,Guid id)
        {
            var productImage = await _repository.GetDataById(token, id);
            if (productImage != null)
            {
                _repository.DeleteData(productImage);
            }
        }
    }
}
