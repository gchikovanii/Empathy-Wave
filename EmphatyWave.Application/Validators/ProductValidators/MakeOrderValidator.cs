using EmphatyWave.Application.Commands.Orders;
using FluentValidation;

namespace EmphatyWave.Application.Validators.ProductValidators
{
    public class MakeOrderValidator : AbstractValidator<MakeOrderCommand>
    {
        public MakeOrderValidator()
        {
            RuleFor(i => i.ShippingDetails.Address).NotEmpty().WithMessage("Address is Required")
                .MinimumLength(5).WithMessage("Min lenght is 5");
            RuleFor(i => i.ShippingDetails.ZipCode).NotEmpty().WithMessage("Zip Code is Required")
               .MinimumLength(4).WithMessage("Min lenght is 5").Matches(@"^\d+$").WithMessage("Only numbers");
            RuleFor(i => i.ShippingDetails.PhoneNumber).NotEmpty().WithMessage("Phone Number is Required")
              .MaximumLength(15).WithMessage("Max lenght is 15").Matches(@"^\d+$").WithMessage("Only numbers");
            RuleFor(i => i.ShippingDetails.CountryCode).NotEmpty().WithMessage("Country Code is Required")
              .MaximumLength(3).WithMessage("Max lenght is 3");
        }
    }
}
