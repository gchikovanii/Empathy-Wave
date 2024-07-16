using EmphatyWave.Application.Queries.Products.DTOs;
using MediatR;

namespace EmphatyWave.Application.Queries.Products
{
    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public Guid Id { get; set; }
    }
}
