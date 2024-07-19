using EmphatyWave.Application.Services.PromoCodes.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Application.Services.PromoCodes.Abstraction
{
    public interface IPromoCodeService
    {
        Task<ICollection<PromoCodeDto>> GetAllPromoCodes(CancellationToken token);
        Task<Result> ApplyPromoCode(CancellationToken token, ApplyPromoCodeDto promoCode);
        Task<Result> ApplyPromoCodes(CancellationToken token, Guid promoCodeId, DateTimeOffset dateTime);
        Task<Result> DeletePromoCode(CancellationToken token, Guid promoCodeId);
        Task<RedemedPromoCodeDto> UserHasRedeemedPromoCodeAsync(CancellationToken token, string userId, Guid promoCodeId);
        Task<ICollection<PromoCodeDto>> GetAvaliablePromoCodse(CancellationToken token, Guid userId);
        Task<UserPromoCode> RedeemPromoCodeAsync(CancellationToken token, string userId, Guid promoCodeId);

    }
}
