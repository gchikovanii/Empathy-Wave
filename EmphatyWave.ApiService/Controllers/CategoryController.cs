using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Application.Queries.Categories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmphatyWave.ApiService.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class CategoryController(IMediator mediator, ILogger<CategoryController> logger) : BaseController
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<CategoryController> _logger = logger;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            return Ok(await _mediator.Send(new GetCategoriesQuery()).ConfigureAwait(false));
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryCommand command)
        {
            var success = await _mediator.Send(command).ConfigureAwait(false);
            return Ok(success);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var success = await _mediator.Send(new DeleteCategoryCommand { Id = id });
            if (success.IsFailure)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
