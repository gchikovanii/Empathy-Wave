using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Application.Queries.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
            try
            {
                var result = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
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
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                return Ok(await _mediator.Send(new GetCategoriesQuery()).ConfigureAwait(false));

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryCommand command)
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var success = await _mediator.Send(new DeleteCategoryCommand { Id = id });
            if (success.IsFailure)
            {
                _logger.LogWarning("Failed to create category with command {@Command}", id);

                return NotFound();
            }
            return Ok();
        }
    }
}
