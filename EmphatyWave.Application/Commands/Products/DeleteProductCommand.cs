using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class DeleteProductCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
