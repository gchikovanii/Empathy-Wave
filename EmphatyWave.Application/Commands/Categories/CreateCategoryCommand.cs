using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class CreateCategoryCommand : IRequest<bool>
    {
        public string Name { get; set; }
    }
}
