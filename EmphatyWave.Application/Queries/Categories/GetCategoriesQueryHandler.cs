using EmphatyWave.Application.Queries.Categories.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Queries.Categories
{
    public class GetCategoriesQueryHandler(ICategoryRepository repo) : IRequestHandler<GetCategoriesQuery, ICollection<CategoryDto>>
    {
        private readonly ICategoryRepository _repo = repo;
        public async Task<ICollection<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetCategories(cancellationToken).ConfigureAwait(false);
            return result.Adapt<ICollection<CategoryDto>>();
        }
    }
}
