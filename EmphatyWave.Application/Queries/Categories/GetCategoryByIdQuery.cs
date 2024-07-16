using EmphatyWave.Application.Queries.Categories.DTOs;
using MediatR;

namespace EmphatyWave.Application.Queries.Categories
{
    public class GetCategoryByIdQuery : IRequest<CategoryDto>
    {
        public Guid Id { get; set; }
    }
}
