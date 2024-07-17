using EmphatyWave.Domain;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class UpdateOrderCommand : IRequest<bool>
    {
        public Status Status { get; set; }
        public Guid Id { get; set; }
        public string UserId { get; set; }
    }
}
