using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Implementation;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class CreateProductCommandHandler(ProductRepository repository, IValidator<CreateProductCommand> validator,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, bool>
    {
        private readonly ProductRepository _repository= repository;
        private readonly IUnitOfWork _unitOfWork= unitOfWork;
        private readonly IValidator<CreateProductCommand> _validator = validator;

        public async Task<bool> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request,cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
            await _repository.CreateProductAsync(cancellationToken, request.Adapt<Product>()).ConfigureAwait(false);
            return await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
