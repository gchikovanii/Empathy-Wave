using EmphatyWave.Application.Queries.Orders.DTOs;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Queries.Orders
{
    public class GetOrdersQueryHandler(IOrderRepository repo) : IRequestHandler<GetOrdersQuery, List<OrderDto>>
    {
        private readonly IOrderRepository _repo = repo;
        public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetOrders(cancellationToken, request.PageNumber, request.PageSize).ConfigureAwait(false);
            return result.Adapt<List<OrderDto>>();
        }
    }
}
