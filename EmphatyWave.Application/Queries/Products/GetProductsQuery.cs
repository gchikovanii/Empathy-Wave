using EmphatyWave.Application.Queries.Products.DTOs;
using EmphatyWave.Persistence.Models;
using MediatR;

namespace EmphatyWave.Application.Queries.Products
{
    public class GetProductsQuery : IRequest<PagedResult<ProductDto>>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? CategoryName { get; set; }
        public string? SearchKeyword { get; set; }
    }
}
