using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IUserPromoCodeRepository
    {
        Task<UserPromoCode> CheckIfUserHasPromoCode(CancellationToken token, Guid promoCodeId, string userId);
        Task<ICollection<UserPromoCode>> GetUserPromoCodes(CancellationToken token, Guid promoCodeId);
        Task ApplyPromoCode(CancellationToken token, UserPromoCode userPromo);
        void DeletePromoCode(UserPromoCode userPromo);
    }
}
