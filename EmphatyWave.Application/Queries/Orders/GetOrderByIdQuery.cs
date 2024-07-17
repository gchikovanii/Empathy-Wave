using EmphatyWave.Application.Queries.Orders.DTOs;
using MediatR;

namespace EmphatyWave.Application.Queries.Orders
{
    public class GetOrderByIdQuery : IRequest<OrderDto>
    {
        public Guid Id { get; set; }
    }
}
