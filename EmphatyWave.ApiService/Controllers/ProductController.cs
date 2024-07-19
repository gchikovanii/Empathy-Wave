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
            try
            {
                var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [OutputCache(Duration = 7200)]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts(int pageSize, int pageNumber)
        {
            try
            {
                return Ok(await _mediator.Send(new GetProductsQuery { PageNumber = pageNumber, PageSize = pageSize }).ConfigureAwait(false));

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductCommand command)
        {
            try
            {
                var success = await _mediator.Send(command).ConfigureAwait(false);
                return Ok(success);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
            var success = await _mediator.Send(new DeleteProductCommand { Id = id });
            if (!success)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
