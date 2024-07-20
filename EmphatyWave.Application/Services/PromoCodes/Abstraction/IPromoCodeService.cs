using EmphatyWave.Application.Services.PromoCodes.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Application.Services.PromoCodes.Abstraction
{
    public interface IPromoCodeService
    {
        Task<ICollection<PromoCodeDto>> GetAllPromoCodes(CancellationToken token);
        Task<ICollection<UserPromoCodeDto>> GetCurrentUserPromoCodes(CancellationToken token, string userId);
        Task<Result> IssuePromoCode(CancellationToken token, PromoCodeDto promoCodeDto);
        Task<Result> ApplyPromoCodes(CancellationToken token, Guid promoCodeId, DateTimeOffset? dateTime);
        Task<Result> ApplyPromoCode(CancellationToken token, ApplyPromoCodeDto promoCode);
        Task<Result> DeletePromoCode(CancellationToken token, string promoCodeToDelete);
        Task<ICollection<PromoCodeDto>> GetAvaliablePromoCodes(CancellationToken token, string userId);
        Task<UserPromoCode> GetPromoCode(CancellationToken token, string userId, Guid promoCodeId);
        Task<PromoCode> GetPromoCodeInfo(CancellationToken token, Guid promoCodeId);
        Task<UserPromoCode> RedeemPromoCodeAsync(CancellationToken token, string userId, Guid promoCodeId);
        Task<PromoCode> GetPromoCodeByPromoName(CancellationToken token, string promoName);
        Task ChangeStatusoOfPromoCode(CancellationToken cancellationToken);
    }
}
