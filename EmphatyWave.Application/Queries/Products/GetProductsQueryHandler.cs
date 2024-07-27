using EmphatyWave.Application.Queries.Products.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Models;
using EmphatyWave.Persistence.Repositories.Abstraction;
using MediatR;

namespace EmphatyWave.Application.Queries.Products
{
    public class GetProductsQueryHandler(IProductRepository repo) : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _repo = repo;
        public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            if(request.MinPrice > request.MaxPrice)
                request.MinPrice = 0;
            var result = await _repo.GetFilteredProducts(cancellationToken, request.PageNumber, request.PageSize,
        request.MinPrice, request.MaxPrice, request.CategoryName, request.SearchKeyword
    ).ConfigureAwait(false);

            var productDtos = result.Items.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Discount = p.Discount,
                Quantity = p.Quantity,
                SKU = p.SKU,
                Rating = p.Rating,
                Images = p.Images.Select(i => new ProductImage
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList(),
                CategoryId = p.Category.Id
            }).ToList();
            return new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = result.TotalCount
            };
        }
    }
}
