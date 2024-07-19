using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class DeleteProductCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
