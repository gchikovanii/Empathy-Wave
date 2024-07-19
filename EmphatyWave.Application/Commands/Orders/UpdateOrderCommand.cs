using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using MediatR;

namespace EmphatyWave.Application.Commands.Orders
{
    public class UpdateOrderCommand : IRequest<Result>
    {
        public Status Status { get; set; }
        public Guid Id { get; set; }
        public string UserId { get; set; }
    }
}
