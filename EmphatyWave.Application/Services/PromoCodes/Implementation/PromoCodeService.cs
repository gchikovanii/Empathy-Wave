using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Services.PromoCodes.Abstraction;
using EmphatyWave.Application.Services.PromoCodes.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.PromoCodeErrors;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmphatyWave.Application.Services.PromoCodes.Implementation
{
    public class PromoCodeService(IPromoCodeRepository promoCodeRepo, IUnitOfWork unit, UserManager<User> userManager,
        IUserPromoCodeRepository userPromoRepo, ILogger<PromoCodeService> logger, IValidator<PromoCodeDto> validator
        ) : IPromoCodeService
    {
        private readonly IValidator<PromoCodeDto> _validator = validator;
        private readonly IPromoCodeRepository _promoCodeRepository = promoCodeRepo;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IUserPromoCodeRepository _userPromoRepo = userPromoRepo;
        private readonly IUnitOfWork _unit = unit;
        private readonly ILogger<PromoCodeService> _logger = logger;
        public async Task<ICollection<PromoCodeDto>> GetAllPromoCodes(CancellationToken token)
        {
            var result = await _promoCodeRepository.GetPromoCodes(token).ConfigureAwait(false);
            return result.Adapt<ICollection<PromoCodeDto>>();
        }
        #region Admin Panel Promo Code
        public async Task<ICollection<UserPromoCode>> GetCurrentUserPromoCodes(CancellationToken token, string userId)
        {
            //UserPromoCodeDto to Return
            var userPromo = await _userPromoRepo.GetCurrentUserPromoCodes(token, userId);
            return userPromo;
        }
        public async Task<Result> IssuePromoCode(CancellationToken token, PromoCodeDto promoCodeDto)
        {
            var dateTime = DateTime.UtcNow;
            if(dateTime >= promoCodeDto.ExpirationDate)
                return Result.Failure(PromoCodeErrors.PromoCodeIncorrectTimeError);
            FluentValidation.Results.ValidationResult result = await _validator.ValidateAsync(promoCodeDto, token).ConfigureAwait(false);

            if (!result.IsValid)
            {
                var errorMessages = result.Errors.Select(e => e.ErrorMessage);
                string errorMessage = string.Join("; ", errorMessages);
                Error error = new("ValidationError", errorMessage);
                return Result.Failure(error);
            }
            var promoCode = new PromoCode
            {
                Id = Guid.NewGuid(),
                Description = promoCodeDto.Description,
                DiscountPercentage = promoCodeDto.DiscountPercentage,
                ExpirationDate = promoCodeDto.ExpirationDate,
                IsActive = promoCodeDto.IsActive,
                Name = promoCodeDto.Name
            };
            await _promoCodeRepository.CreatePromoCodeAsync(token, promoCode).ConfigureAwait(false);
            var res = await _unit.SaveChangesAsync(token).ConfigureAwait(false);
            if (res == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }
        public async Task<Result> ApplyPromoCodes(CancellationToken token, Guid promoCodeId, DateTimeOffset? dateTime)
        {
            var query = _userManager.Users.AsQueryable();
            if (dateTime.HasValue)
            {
                query = query.Where(i => i.VerifiedAt <= dateTime.Value);
            }
            var users = await query.ToListAsync(token).ConfigureAwait(false);
            if(users.Count() == 0)
                return Result.Success();
            foreach (var user in users)
            {
                var userPromo = await _userPromoRepo.CheckIfUserHasPromoCode(token, promoCodeId, user.Id);
                if (userPromo != null && userPromo.UserId != null) continue;
                var newUserPromo = new UserPromoCode { UserId = user.Id, PromoCodeId = promoCodeId };
                await _userPromoRepo.ApplyPromoCode(token, newUserPromo);
            }
            var res = await _unit.SaveChangesAsync(token).ConfigureAwait(false);
            if (res == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }

        public async Task<Result> ApplyPromoCode(CancellationToken token, ApplyPromoCodeDto promoCode)
        {
            var userPromo = await _userPromoRepo.CheckIfUserHasPromoCode(token, promoCode.PromoCodeId, promoCode.UserId);
            if (userPromo != null && userPromo.UserId != null)
                return Result.Failure(PromoCodeErrors.PromoCodeAlreadyExists);
            await _userPromoRepo.ApplyPromoCode(token, promoCode.Adapt<UserPromoCode>());
            var res = await _unit.SaveChangesAsync(token).ConfigureAwait(false);
            if (res == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }
        public async Task<Result> DeletePromoCode(CancellationToken token, string promoCodeToDelete)
        {
            try
            {
                var promoCode = await _promoCodeRepository.GetPromoCodeByPromoCode(token, promoCodeToDelete).ConfigureAwait(false);
                if (promoCode == null)
                    return Result.Failure(PromoCodeErrors.PromoCodeNotFound);
                _promoCodeRepository.DeletePromoCode(promoCode);
                var userPromoCodes = await _userPromoRepo.GetPromoCodesById(token, promoCode.Id).ConfigureAwait(false);
                foreach (var pCode in userPromoCodes)
                {
                    _userPromoRepo.DeletePromoCode(pCode);
                }
                var res = await _unit.SaveChangesAsync(token).ConfigureAwait(false);
                if (res == false)
                    return Result.Failure(UnitError.CantSaveChanges);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting promo code {PromoCode}", promoCodeToDelete);
                return Result.Failure(UnitError.UnexcpectedError);
            }
        }
        #endregion

        #region Users
        public async Task<ICollection<PromoCodeDto>> GetAvaliablePromoCodes(CancellationToken token, string userId)
        {
            var userPromo = await _userPromoRepo.GetCurrentUserPromoCodes(token, userId).ConfigureAwait(false);
            if (userPromo == null || !userPromo.Any())
            {
                return new List<PromoCodeDto>();
            }
            List<PromoCodeDto> promoCodeDtos = new List<PromoCodeDto>();
            foreach (var uPromo in userPromo)
            {
                var promoCode = await _promoCodeRepository.GetPromoCodeById(token, uPromo.PromoCodeId).ConfigureAwait(false);
                if (promoCode != null)
                {
                    promoCodeDtos.Add(promoCode.Adapt<PromoCodeDto>());
                }
            }
            return promoCodeDtos;
        }

        public async Task<UserPromoCode> GetPromoCode(CancellationToken token, string userId, Guid promoCodeId)
        {
            var userPromo = await _userPromoRepo.GetUserPromoCode(token, promoCodeId,userId).ConfigureAwait(false);
            return userPromo;
        }
        public async Task<PromoCode> GetPromoCodeInfo(CancellationToken token, Guid promoCodeId)
        {
            var promoCode = await _promoCodeRepository.GetPromoCodeById(token, promoCodeId).ConfigureAwait(false);
            return promoCode;
        }
        public async Task<PromoCode> GetPromoCodeByPromoName(CancellationToken token, string promoName)
        {
            var promoCode = await _promoCodeRepository.GetPromoCodeByPromoCode(token, promoName).ConfigureAwait(false);
            return promoCode;
        }
        public async Task<UserPromoCode> RedeemPromoCodeAsync(CancellationToken token, string userId, Guid promoCodeId)
        {
            var userPromo = await _userPromoRepo.CheckIfUserHasPromoCode(token, promoCodeId, userId).ConfigureAwait(false);
            userPromo.RedeemedAt = DateTime.UtcNow;
            return userPromo;
        }
        #endregion
    }
}
