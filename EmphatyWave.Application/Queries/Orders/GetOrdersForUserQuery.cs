using EmphatyWave.Application.Queries.Orders.DTOs;
using MediatR;

namespace EmphatyWave.Application.Queries.Orders
{
    public class GetOrdersForUserQuery : IRequest<List<OrderDto>>
    {
        public string UserId { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}