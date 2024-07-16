using EmphatyWave.Application.Queries.Products.DTOs;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.Repositories.Implementation;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Queries.Products
{
    public class GetProductByIdQueryHandler(IProductRepository repo) : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        private readonly IProductRepository _repo = repo;
        public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetProductById(cancellationToken,request.Id).ConfigureAwait(false);
            return result.Adapt<ProductDto>();  
        }
    }
}
