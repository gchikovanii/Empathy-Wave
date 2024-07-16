using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class CreateProductCommandHandler(IProductRepository repository, IValidator<CreateProductCommand> validator
        ) : IRequestHandler<CreateProductCommand, bool>
    {
        private readonly IProductRepository _repository = repository;
        private readonly IValidator<CreateProductCommand> _validator = validator;

        public async Task<bool> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
            var res = await _repository.CreateProductAsync(cancellationToken, request.Adapt<Product>()).ConfigureAwait(false);
            return res;
        }
    }
}
