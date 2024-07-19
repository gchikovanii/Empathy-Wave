using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class CreateCategoryCommand : IRequest<Result>
    {
        public string Name { get; set; }
    }
}
