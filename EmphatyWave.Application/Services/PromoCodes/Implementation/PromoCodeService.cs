using EmphatyWave.Application.Services.PromoCodes.Abstraction;
using EmphatyWave.Application.Services.PromoCodes.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.PromoCodeErrors;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Application.Services.PromoCodes.Implementation
{
    public class PromoCodeService(IPromoCodeRepository promoCodeRepo, IUnitOfWork unit, UserManager<User> userManager,
        IUserPromoCodeRepository userPromoRepo
        ) : IPromoCodeService
    {
        private readonly IPromoCodeRepository _promoCodeRepository = promoCodeRepo;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IUserPromoCodeRepository _userPromoRepo = userPromoRepo;
        private readonly IUnitOfWork _unit = unit;
        public async Task<ICollection<PromoCodeDto>> GetAllPromoCodes(CancellationToken token)
        {
            var result = await _promoCodeRepository.GetPromoCodes(token).ConfigureAwait(false);
            return result.Adapt<ICollection<PromoCodeDto>>();
        }
        public async Task<Result> IssuePromoCode(CancellationToken token,PromoCodeDto promoCodeDto)
        {
            var promoCode = new PromoCode
            {
                Id = Guid.NewGuid(),
                Description = promoCodeDto.Description,
                DiscountPercentage = promoCodeDto.DiscountPercentage,   
                ExpirationDate = DateTime.UtcNow,
                IsActive = promoCodeDto.IsActive,   
                Name = promoCodeDto.Name
            };
            await _promoCodeRepository.CreatePromoCodeAsync(token, promoCode).ConfigureAwait(false);
            var res = await _unit.SaveChangesAsync(token).ConfigureAwait(false);
            if (res == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }


        public async Task<Result> ApplyPromoCode(CancellationToken token,ApplyPromoCodeDto promoCode)
        {
            await _userPromoRepo.ApplyPromoCode(token, promoCode.Adapt<UserPromoCode>());
            var res = await _unit.SaveChangesAsync(token).ConfigureAwait(false);
            if (res == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }
        public async Task<Result> ApplyPromoCodes(CancellationToken token,Guid promoCodeId, DateTimeOffset dateTime)
        {
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var userPromo = await _userPromoRepo.CheckIfUserHasPromoCode(token, promoCodeId, user.Id);
                if(userPromo == null) continue;
                var newUserPromo = new UserPromoCode { UserId = user.Id, PromoCodeId = promoCodeId };
                await _userPromoRepo.ApplyPromoCode(token, newUserPromo);
            }
            var res = await _unit.SaveChangesAsync(token).ConfigureAwait(false);
            if (res == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }

        public async Task<Result> DeletePromoCode(CancellationToken token, Guid promoCodeId)
        {
            var promoCode = await _promoCodeRepository.GetPromoCodeById(token, promoCodeId).ConfigureAwait(false);
            if (promoCode == null)
                return Result.Failure(PromoCodeErrors.PromoCodeNotFound);
            _promoCodeRepository.DeletePromoCode(promoCode);
            var userPromoCodes = await _userPromoRepo.GetUserPromoCodes(token, promoCodeId).ConfigureAwait(false);
            foreach (var pCode in userPromoCodes)
            {
                _userPromoRepo.DeletePromoCode(pCode);
            }
            var res = await _unit.SaveChangesAsync(token).ConfigureAwait(false);
            if (res == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }
        public async Task<RedemedPromoCodeDto> UserHasRedeemedPromoCodeAsync(CancellationToken token,string userId, Guid promoCodeId)
        {
            var userPromo = await _userPromoRepo.CheckIfUserHasPromoCode(token, promoCodeId, userId);
            return userPromo.Adapt<RedemedPromoCodeDto>();
        }
        //For User
        public async Task<ICollection<PromoCodeDto>> GetAvaliablePromoCodse(CancellationToken token,Guid userId)
        {
            var userPromo = await _userPromoRepo.GetUserPromoCodes(token, userId).ConfigureAwait(false);
            return userPromo.Adapt<ICollection<PromoCodeDto>>();
        }
        public async Task<UserPromoCode> RedeemPromoCodeAsync(CancellationToken token, string userId,Guid promoCodeId)
        {
            var userPromo = await _userPromoRepo.CheckIfUserHasPromoCode(token, promoCodeId, userId).ConfigureAwait(false);
            userPromo.RedeemedAt = DateTime.UtcNow;
            await _unit.SaveChangesAsync(token).ConfigureAwait(false);
            return userPromo;
        }

    }
}
