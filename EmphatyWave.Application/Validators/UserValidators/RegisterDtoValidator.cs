using EmphatyWave.Application.Services.Account.DTOs;
using EmphatyWave.Domain.Localization;
using FluentValidation;

namespace EmphatyWave.Application.Validators.UserValidators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(i => i.Email).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
                .EmailAddress().WithMessage(ErrorMessages.Email);
            RuleFor(i => i.Password)
            .NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$")
            .WithMessage(ErrorMessages.Password);
        }
    }
}
