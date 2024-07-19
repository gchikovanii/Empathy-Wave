using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Categories;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class CreateProductCommandHandler(IProductRepository repository, ICategoryRepository categoryRepo,
        IValidator<CreateProductCommand> validator,IUnitOfWork unit
        ) : IRequestHandler<CreateProductCommand, Result>
    {
        private readonly ICategoryRepository _categoryRepo = categoryRepo;
        private readonly IProductRepository _repository = repository;
        private readonly IUnitOfWork _unit = unit;
        private readonly IValidator<CreateProductCommand> _validator = validator;

        public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
            {
                var errorMessages = result.Errors.Select(e => e.ErrorMessage);
                string errorMessage = string.Join("; ", errorMessages);
                Error error = new("ValidationError", errorMessage);
                return Result.Failure(error);
            }
            var category = await _categoryRepo.GetCategoryById(cancellationToken, request.CategoryId).ConfigureAwait(false);
            if (category == null)
                return Result.Failure(CategoryErrors.CategoryNotExists);
            await _repository.CreateProductAsync(cancellationToken, request.Adapt<Product>()).ConfigureAwait(false);
            var saves = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (saves == false)
            {
                return Result.Failure(UnitError.CantSaveChanges);
            }
            return Result.Success();
        }
    }
}
