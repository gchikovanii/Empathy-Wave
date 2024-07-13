using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly IBaseRepository<ProductImage> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductImageRepository(IBaseRepository<ProductImage> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ICollection<ProductImage>> GetImages(CancellationToken token, Guid productId)
        {
            return await _repository.GetQuery(i => i.ProductId == productId).ToListAsync(token).ConfigureAwait(false);
        }
        public async Task<bool> CreateOrderImageAsync(CancellationToken token,ProductImage productImage)
        {
            await _repository.CreateData(token, productImage).ConfigureAwait(false);
            return await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
        }

        public async Task DeleteOrderImage(CancellationToken token,Guid id)
        {
            var productImage = await _repository.GetDataById(token, id);
            if (productImage != null)
            {
                _repository.DeleteData(productImage);
                await _unitOfWork.SaveChangesAsync(token).ConfigureAwait(false);
            }
        }
    }
}
