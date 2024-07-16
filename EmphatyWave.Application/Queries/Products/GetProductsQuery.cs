using EmphatyWave.Application.Queries.Products.DTOs;
using MediatR;

namespace EmphatyWave.Application.Queries.Products
{
    public class GetProductsQuery : IRequest<ICollection<ProductDto>>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
