using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IUserPromoCodeRepository
    {
        Task<UserPromoCode> CheckIfUserHasPromoCode(CancellationToken token, Guid promoCodeId, string userId);
        Task<ICollection<UserPromoCode>> GetUserPromoCodes(CancellationToken token, string userId);
        Task ApplyPromoCode(CancellationToken token, UserPromoCode userPromo);
        void DeletePromoCode(UserPromoCode userPromo);
        Task<ICollection<UserPromoCode>> GetCurrentUserPromoCodes(CancellationToken token, string userId);
        Task<ICollection<UserPromoCode>> GetPromoCodesById(CancellationToken token, Guid promoCodeId);
        Task<UserPromoCode> GetUserPromoCode(CancellationToken token, Guid promoCodeId, string userId);
    }
}
