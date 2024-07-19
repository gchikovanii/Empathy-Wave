using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Domain;
using EmphatyWave.Domain.Localization;
using FluentValidation;

namespace EmphatyWave.Application.Validators.ProductValidators
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(i => i.SKU).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired).MinimumLength(5).WithMessage($"{ErrorMessages.MinLengthIs} 5");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
                .MinimumLength(15).WithMessage(ErrorMessages.DescriptionSize);
            RuleFor(i => i.Price)
                .NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
                .InclusiveBetween(1, 35000).WithMessage(ErrorMessages.PriceRange);
            RuleFor(i => i.Discount).Must((command,discount) => discount.HasValue ? discount.Value >= 0 && discount.Value <= command.Price : true)
                .WithMessage(ErrorMessages.DiscountRange);
            RuleFor(i => i.Name)
                .NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
                .MinimumLength(3).WithMessage($"{ErrorMessages.MinLengthIs} 3");
            RuleFor(i => i.Title)
                .NotEmpty().WithMessage(ErrorMessages.FieldIsRequired)
                .MaximumLength(35).WithMessage($"{ErrorMessages.MaxLengthIs} 35");
        }
    }
}
