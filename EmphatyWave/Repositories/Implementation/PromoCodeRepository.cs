using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class PromoCodeRepository(IBaseRepository<PromoCode> repo) : IPromoCodeRepository
    {
        private readonly IBaseRepository<PromoCode> _repo = repo;

        public async Task<PromoCode> GetPromoCodeById(CancellationToken token, Guid productId)
        {
            return await _repo.GetDataById(token, productId).ConfigureAwait(false);
        }
        public async Task<PromoCode> GetPromoCodeByPromoCode(CancellationToken token, string promoCode)
        {
            return await _repo.GetQuery(i => i.Name == promoCode).FirstOrDefaultAsync(token).ConfigureAwait(false) ?? new PromoCode { };
        }
        public async Task<ICollection<PromoCode>> GetPromoCodes(CancellationToken token)
        {
            return await _repo.GetQuery().ToListAsync(token).ConfigureAwait(false);
        }
        public async Task CreatePromoCodeAsync(CancellationToken token, PromoCode product)
        {
            await _repo.CreateData(token, product).ConfigureAwait(false);
        }
        public void DeletePromoCode(PromoCode promoCode)
        {
            _repo.DeleteData(promoCode);
        }

        public async Task<ICollection<PromoCode>> ChangeStatus(CancellationToken token)
        {
            var date = DateTime.UtcNow;
            var promoCodes = await _repo.GetQuery(i => i.IsActive == true && i.ExpirationDate >= date).ToListAsync(token).ConfigureAwait(false);
            return promoCodes;
        }
    }
}
