using EmphatyWave.Persistence.Repositories.Implementation;
using EmphatyWave.Persistence.UOW;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class DeleteProductCommandHandler(ProductRepository repo, IUnitOfWork unit) : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly ProductRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            await _repo.DeleteProduct(cancellationToken,request.Id).ConfigureAwait(false);
            return await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
