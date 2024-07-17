using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class DeleteOrderCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
    }
}
