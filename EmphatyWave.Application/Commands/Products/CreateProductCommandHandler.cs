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
    public class CreateProductCommandHandler(IProductRepository repository) : IRequestHandler<CreateProductCommand, bool>
    {
        private readonly IProductRepository _repository = repository;

        public async Task<bool> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.CreateProductAsync(cancellationToken, request.Adapt<Product>()).ConfigureAwait(false);
            return result;
        }
    }
}
