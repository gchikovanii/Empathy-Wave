using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class DeleteCategoryCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
