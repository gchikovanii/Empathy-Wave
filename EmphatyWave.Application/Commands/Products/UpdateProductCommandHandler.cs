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
    public class UpdateProductCommandHandler(IProductRepository repo) : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _repo = repo;
        //private readonly IValidator<UpdateProductCommand> _validator = validator; 
        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            //ValidationResult result = await _validator.ValidateAsync(request,cancellationToken).ConfigureAwait(false);
            //if (!result.IsValid)
            //    throw new ValidationException(result.Errors);
            var result = await _repo.UpdateProduct(cancellationToken,request.Adapt<Product>()).ConfigureAwait(false); 
            return result;
        }
    }
}
