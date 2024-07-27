using EmphatyWave.ApiService.Infrastructure.Request;
using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace EmphatyWave.ApiService.Controllers
{
    public class ProductController(IMediator mediator) : BaseController
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
            if (result == null)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [OutputCache(Duration = 7200)]
        [HttpGet]

        public async Task<IActionResult> GetFilteredProducts(int pageNumber, int pageSize, decimal? minPrice, decimal? maxPrice, string? categoryName, string? searchKeyword)
        {
            return Ok(await _mediator.Send(new GetProductsQuery { PageNumber = pageNumber, PageSize = pageSize,
                MinPrice = minPrice, MaxPrice = maxPrice,
                CategoryName = categoryName, SearchKeyword = searchKeyword
            }));
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductCommand command)
        {
            return Ok(await _mediator.Send(command).ConfigureAwait(false));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("Product ID mismatch.");
            }
            var success = await _mediator.Send(command).ConfigureAwait(false);
            return Ok(success);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            return Ok(await _mediator.Send(new DeleteProductCommand { Id = id }));
        }
    }
}
