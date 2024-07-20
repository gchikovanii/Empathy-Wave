using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IPromoCodeRepository
    {
        Task<ICollection<PromoCode>> GetPromoCodes(CancellationToken token);
        Task<PromoCode> GetPromoCodeById(CancellationToken token, Guid promoCodeId);
        Task CreatePromoCodeAsync(CancellationToken token, PromoCode promoCode);
        Task<PromoCode> GetPromoCodeByPromoCode(CancellationToken token, string promoCode);
        void DeletePromoCode(PromoCode promoCode);
        Task<ICollection<PromoCode>> ChangeStatus(CancellationToken token);
    }
}
