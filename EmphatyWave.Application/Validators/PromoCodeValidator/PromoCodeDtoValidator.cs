using EmphatyWave.Application.Services.Account.DTOs;
using EmphatyWave.Application.Services.PromoCodes.DTOs;
using EmphatyWave.Domain.Localization;
using FluentValidation;


namespace EmphatyWave.Application.Validators.PromoCodeValidator
{
    internal class PromoCodeDtoValidator : AbstractValidator<PromoCodeDto>
    {
        public PromoCodeDtoValidator()
        {
            RuleFor(i => i.Name).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired);
            RuleFor(i => i.DiscountPercentage).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired);
            RuleFor(i => i.Description).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired);
            RuleFor(i => i.ExpirationDate).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired);
        }
    }
}
