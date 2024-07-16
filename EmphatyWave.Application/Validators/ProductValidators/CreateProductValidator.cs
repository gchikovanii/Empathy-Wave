using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Domain;
using FluentValidation;

namespace EmphatyWave.Application.Validators.ProductValidators
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(i => i.SKU).NotEmpty().WithMessage("SKU is Required").MinimumLength(5).WithMessage("Min lenght is 5");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MinimumLength(15).WithMessage("Description must contain at least 15 characters.");
            RuleFor(i => i.Price)
                .NotEmpty().WithMessage("Price is Required")
                .InclusiveBetween(1, 35000).WithMessage("Price must be between 1 and 35K");
            RuleFor(i => i.Discount).Must((command,discount) => discount.HasValue ? discount.Value >= 0 && discount.Value <= command.Price : true)
                .WithMessage("Discount must be between 1 and the product price.");
            RuleFor(i => i.Name)
                .NotEmpty().WithMessage("Name is Required")
                .MinimumLength(2).WithMessage("Min Length is two.");
            RuleFor(i => i.Title)
                .NotEmpty().WithMessage("Title is Required")
                .MaximumLength(35).WithMessage("Max Length is 35.");
        }
    }
}
