using EmphatyWave.ApiService.Infrastructure.Extensions;
using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Queries.Orders;
using EmphatyWave.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmphatyWave.ApiService.Controllers
{
    public class OrderController(IMediator mediator, UserManager<User> userManager) : BaseController
    {
        private readonly IMediator _mediator = mediator;
        private readonly UserManager<User> _userManager = userManager;

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var result = await _mediator.Send(new GetOrderByIdQuery { Id = id });
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(int pageSize, int pageNumber)
        {
            return Ok(await _mediator.Send(new GetOrdersQuery { PageNumber = pageNumber, PageSize = pageSize }).ConfigureAwait(false));

        }
        [Authorize]
        [HttpGet("GetAllOrdersForUser")]
        public async Task<IActionResult> GetAllOrdersForUser(int pageSize, int pageNumber, string UserId)
        {
            return Ok(await _mediator.Send(new GetOrdersForUserQuery { PageNumber = pageNumber, PageSize = pageSize, UserId = UserId }).ConfigureAwait(false));

        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MakeOrder(MakeOrderCommand command)
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(User);
            command.UserId = user.Id;
            var success = await _mediator.Send(command).ConfigureAwait(false);
            return Ok(success);
        }
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateOrder(UpdateOrderCommand command)
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(User);
            command.UserId = user.Id;
            var success = await _mediator.Send(command).ConfigureAwait(false);
            return Ok(success);
        }
        [Authorize]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id, string userId)
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(User);
            userId = user.Id;
            return Ok(await _mediator.Send(new DeleteOrderCommand { Id = id, UserId = userId }));
        }
    }
}
