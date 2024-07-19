using EmphatyWave.Application.Commands.Orders.DTOs;
using EmphatyWave.Application.Commands.Orders.Models;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class MakeOrderCommand : IRequest<Result>
    {
        public List<OrderItemDto> OrderItems { get; set; }
        public string UserId { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public ShippingDetail ShippingDetails { get; set; }
    }
}
