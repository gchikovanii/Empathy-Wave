using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class DeleteCategoryCommandHandler(ICategoryRepository repo, IUnitOfWork unit) : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly ICategoryRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            await _repo.DeleteCategory(cancellationToken,request.Id).ConfigureAwait(false);
            return await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false); 
        }
    }
}
