using EmphatyWave.Application.Queries.Orders.DTOs;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Queries.Orders
{
    public class GetOrdersForUserQueryHandler(IOrderRepository repo) : IRequestHandler<GetOrdersForUserQuery, List<OrderDto>>
    {
        private readonly IOrderRepository _repo = repo;
        public async Task<List<OrderDto>> Handle(GetOrdersForUserQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetOrdersForUser(cancellationToken, request.PageNumber, request.PageSize,request.UserId).ConfigureAwait(false);
            return result.Adapt<List<OrderDto>>();
        }
    }
}
