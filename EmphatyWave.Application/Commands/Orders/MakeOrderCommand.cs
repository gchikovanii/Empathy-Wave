using EmphatyWave.Application.Commands.Orders.DTOs;
using EmphatyWave.Domain;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class MakeOrderCommand : IRequest<bool>
    {
        public List<OrderItemDto> OrderItems { get; set; }
        public string UserId { get; set; }
        public ShippingDetail ShippingDetails { get; set; }
    }
}
