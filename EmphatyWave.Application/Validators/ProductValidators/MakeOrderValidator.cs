using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Domain.Localization;
using FluentValidation;

namespace EmphatyWave.Application.Validators.ProductValidators
{
    public class MakeOrderValidator : AbstractValidator<MakeOrderCommand>
    {
        public MakeOrderValidator()
        {
            RuleFor(i => i.ShippingDetails.Address).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
                .MinimumLength(5).WithMessage($"{ErrorMessages.MinLengthIs} 5");
            RuleFor(i => i.ShippingDetails.ZipCode).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
               .MinimumLength(4).WithMessage($"{ErrorMessages.MinLengthIs} 4").Matches(@"^\d+$").WithMessage(ErrorMessages.OnlyNumbers);
            RuleFor(i => i.ShippingDetails.PhoneNumber).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
              .MaximumLength(20).WithMessage($"{ErrorMessages.MaxLengthIs} 20").Matches(@"^\d+$").WithMessage(ErrorMessages.OnlyNumbers);
            RuleFor(i => i.ShippingDetails.CountryCode).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
              .MaximumLength(3).WithMessage($"{ErrorMessages.MinLengthIs} 3");
        }
    }
}
