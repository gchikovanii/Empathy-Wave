using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class CreateCategoryCommand : IRequest<bool>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
