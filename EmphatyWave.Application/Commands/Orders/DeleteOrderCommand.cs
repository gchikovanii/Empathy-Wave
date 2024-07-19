using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class DeleteOrderCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
    }
}
