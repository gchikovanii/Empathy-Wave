using EmphatyWave.Application.Queries.Orders.DTOs;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Queries.Orders
{
    public class GetOrderByIdQueryHandler(IOrderRepository repo) : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IOrderRepository _repo = repo;
        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetOrderById(cancellationToken, request.Id).ConfigureAwait(false);
            return result.Adapt<OrderDto>();
        }
    }
}
