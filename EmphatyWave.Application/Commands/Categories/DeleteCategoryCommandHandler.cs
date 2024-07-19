using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class DeleteCategoryCommandHandler(ICategoryRepository repo, IUnitOfWork unit) : IRequestHandler<DeleteCategoryCommand, Result>
    {
        private readonly ICategoryRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {

            await _repo.DeleteCategory(cancellationToken, request.Id).ConfigureAwait(false);
            var result = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (result == false)
            {
                return Result.Failure(UnitError.CantSaveChanges);
            }
            return Result.Success();
        }
    }
}
