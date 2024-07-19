using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Domain.Localization;
using FluentValidation;

namespace EmphatyWave.Application.Validators.Categories
{
    public class CategoryValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CategoryValidator()
        {
            RuleFor(i => i.Name).NotEmpty().WithMessage(ErrorMessages.FieldIsRequired).MaximumLength(15).WithMessage($"{ErrorMessages.MaxLengthIs} 15");
        }
    }
}
