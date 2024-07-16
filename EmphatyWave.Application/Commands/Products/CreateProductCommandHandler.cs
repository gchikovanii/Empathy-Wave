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
    public class CreateProductCommandHandler(IProductRepository repository, 
        IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, bool>
    {
        private readonly IProductRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork= unitOfWork;

        public async Task<bool> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
           
            await _repository.CreateProductAsync(cancellationToken, request.Adapt<Product>()).ConfigureAwait(false);
            return await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
