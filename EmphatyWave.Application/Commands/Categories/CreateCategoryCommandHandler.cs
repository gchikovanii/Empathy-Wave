using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class CreateCategoryCommandHandler(ICategoryRepository repo, IUnitOfWork unit) : IRequestHandler<CreateCategoryCommand, bool>
    {
        private readonly ICategoryRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        public async Task<bool> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            await _repo.CreateCategoryAsync(cancellationToken, request.Adapt<Category>()).ConfigureAwait(false);
            return await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
