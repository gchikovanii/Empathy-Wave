using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Products;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class UpdateProductCommandHandler(IProductRepository repo, IValidator<UpdateProductCommand> validator, IUnitOfWork unit
        ) : IRequestHandler<UpdateProductCommand, Result>
    {
        private readonly IProductRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        private readonly IValidator<UpdateProductCommand> _validator = validator;
        public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
            {
                var errorMessages = result.Errors.Select(e => e.ErrorMessage);
                string errorMessage = string.Join("; ", errorMessages);
                Error error = new("ValidationError", errorMessage);
                return Result.Failure(error);
            }
            var productExists = await _repo.GetProductById(cancellationToken,request.Id).ConfigureAwait(false);
            if (productExists == null)
                return Result.Failure(ProductErrors.ProductNotFound);
            _repo.UpdateProduct(request.Adapt<Product>());
            var saves = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (saves == false)
            {
                return Result.Failure(UnitError.CantSaveChanges);
            }
            return Result.Success();
        }
    }
}
