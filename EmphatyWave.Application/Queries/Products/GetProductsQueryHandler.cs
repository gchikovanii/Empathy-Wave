using EmphatyWave.Application.Queries.Products.DTOs;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Queries.Products
{
    public class GetProductsQueryHandler(IProductRepository repo) : IRequestHandler<GetProductsQuery, ICollection<ProductDto>>
    {
        private readonly IProductRepository _repo = repo;
        public async Task<ICollection<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetProducts(cancellationToken,request.PageNumber,request.PageSize).ConfigureAwait(false);
            return result.Adapt<ICollection<ProductDto>>();
        }
    }
}
