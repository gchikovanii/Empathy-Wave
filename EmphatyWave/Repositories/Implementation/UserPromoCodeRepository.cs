using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class UserPromoCodeRepository(IBaseRepository<UserPromoCode> repository) : IUserPromoCodeRepository
    {
        private readonly IBaseRepository<UserPromoCode> _repository = repository;
        public async Task<UserPromoCode> CheckIfUserHasPromoCode(CancellationToken token, Guid promoCodeId, string userId)
        {
            var result = await _repository.GetQuery(i => i.PromoCodeId == promoCodeId && i.UserId == userId).FirstOrDefaultAsync(token).ConfigureAwait(false);
            return result ?? new UserPromoCode { };
        }
        public async Task<ICollection<UserPromoCode>> GetPromoCodeByUserId(CancellationToken token, string userId)
        {
            return await _repository.GetQuery(i => i.UserId == userId).ToListAsync(token).ConfigureAwait(false);
        }
        public async Task ApplyPromoCode(CancellationToken token, UserPromoCode userPromo)
        {
            await _repository.CreateData(token, userPromo).ConfigureAwait(false);
        }
        public void DeletePromoCode(UserPromoCode userPromo)
        {
            _repository.DeleteData(userPromo);
        }

        public async Task<ICollection<UserPromoCode>> GetUserPromoCodes(CancellationToken token, Guid promoCodeId)
        {
            return await _repository.GetQuery(i => i.PromoCodeId == promoCodeId && !i.RedeemedAt.HasValue).ToListAsync(token).ConfigureAwait(false);
        }

    }
}
