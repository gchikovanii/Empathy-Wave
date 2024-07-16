using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
