using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface ICategoryRepository
    {
        Task<ICollection<Category>> GetCategories(CancellationToken token);
        Task<Category> GetCategoryById(CancellationToken token, Guid categoryId);
        Task CreateCategoryAsync(CancellationToken token, Category category);
        void DeleteCategory(Category category);
    }
}
