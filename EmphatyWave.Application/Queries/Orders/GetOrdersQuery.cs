using EmphatyWave.Application.Queries.Orders.DTOs;
using MediatR;

namespace EmphatyWave.Application.Queries.Orders
{
    public class GetOrdersQuery : IRequest<List<OrderDto>>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
