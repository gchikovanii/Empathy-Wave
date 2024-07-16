using EmphatyWave.Application.Queries.Categories.DTOs;
using MediatR;

namespace EmphatyWave.Application.Queries.Categories
{
    public class GetCategoriesQuery : IRequest<ICollection<CategoryDto>>
    {
    }
}
