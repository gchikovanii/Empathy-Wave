using EmphatyWave.Application.Validators.ProductValidators;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.Repositories.Implementation;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class UpdateProductCommandHandler(IProductRepository repo, IValidator<UpdateProductCommand> validator, IUnitOfWork unit
        ) : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        private readonly IValidator<UpdateProductCommand> _validator = validator;
        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
            var productExists = await _repo.GetProductById(cancellationToken,request.Id).ConfigureAwait(false);
            if (productExists == null)
                throw new Exception($"Product with this id - {request.Id} doesn't exists!");
            _repo.UpdateProduct(request.Adapt<Product>()); 
            return await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
