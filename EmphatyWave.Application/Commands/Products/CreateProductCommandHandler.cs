using EmphatyWave.Domain;
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
        ) : IRequestHandler<CreateProductCommand, bool>
    {
        private readonly ICategoryRepository _categoryRepo = categoryRepo;
        private readonly IProductRepository _repository = repository;
        private readonly IUnitOfWork _unit = unit;
        private readonly IValidator<CreateProductCommand> _validator = validator;

        public async Task<bool> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
            var category = await _categoryRepo.GetCategoryById(cancellationToken, request.CategoryId).ConfigureAwait(false);
            if (category == null)
                throw new Exception($"Category with this {request.CategoryId} doesn't exists");
            await _repository.CreateProductAsync(cancellationToken, request.Adapt<Product>()).ConfigureAwait(false);
            return await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
