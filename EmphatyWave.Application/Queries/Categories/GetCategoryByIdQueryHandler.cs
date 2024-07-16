using EmphatyWave.Application.Queries.Categories.DTOs;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.Repositories.Implementation;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Queries.Categories
{
    public class GetCategoryByIdQueryHandler(ICategoryRepository repo) : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
    {
        private readonly ICategoryRepository _repo = repo;
        public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetCategoryById(cancellationToken, request.Id).ConfigureAwait(false);
            return result.Adapt<CategoryDto>();
        }
    }
}
