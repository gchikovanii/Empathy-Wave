using EmphatyWave.Persistence.Repositories.Abstraction;
using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class DeleteCategoryCommandHandler(ICategoryRepository repo) : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly ICategoryRepository _repo = repo;
        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            return await _repo.DeleteCategory(cancellationToken,request.Id).ConfigureAwait(false);
        }
    }
}
