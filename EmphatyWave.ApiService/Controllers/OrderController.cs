using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Queries.Orders;
using EmphatyWave.Application.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmphatyWave.ApiService.Controllers
{
    public class OrderController(IMediator mediator) : BaseController
    {
        private readonly IMediator _mediator = mediator;
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetOrderByIdQuery { Id = id });
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(int pageSize, int pageNumber)
        {
            try
            {
                return Ok(await _mediator.Send(new GetOrdersQuery { PageNumber = pageNumber, PageSize = pageSize }).ConfigureAwait(false));

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("GetAllOrdersForUser")]
        public async Task<IActionResult> GetAllOrdersForUser(int pageSize, int pageNumber, string UserId)
        {
            try
            {
                return Ok(await _mediator.Send(new GetOrdersForUserQuery { PageNumber = pageNumber, PageSize = pageSize, UserId = UserId }).ConfigureAwait(false));

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MakeOrder(MakeOrderCommand command)
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
        [Authorize]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, UpdateOrderCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("Order ID mismatch.");
            }
            var success = await _mediator.Send(command).ConfigureAwait(false);
            return Ok(success);
        }
        [Authorize]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var success = await _mediator.Send(new DeleteOrderCommand { Id = id });
            if (!success)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
