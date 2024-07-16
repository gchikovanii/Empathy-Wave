using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class CreateCategoryCommandHandler(ICategoryRepository repo) : IRequestHandler<CreateCategoryCommand, bool>
    {
        private readonly ICategoryRepository _repo = repo;
        public async Task<bool> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            return await _repo.CreateCategoryAsync(cancellationToken, request.Adapt<Category>()).ConfigureAwait(false);
        }
    }
}
