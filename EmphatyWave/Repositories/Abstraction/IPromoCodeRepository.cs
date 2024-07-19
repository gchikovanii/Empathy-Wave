using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Implementation;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IPromoCodeRepository
    {
        Task<ICollection<PromoCode>> GetPromoCodes(CancellationToken token);
        Task<PromoCode> GetPromoCodeById(CancellationToken token, Guid promoCodeId);
        Task CreatePromoCodeAsync(CancellationToken token, PromoCode promoCode);
        void DeletePromoCode(PromoCode promoCode);
    }
}
