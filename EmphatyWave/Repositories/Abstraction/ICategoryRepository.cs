using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface ICategoryRepository
    {
        Task<ICollection<Category>> GetCategories(CancellationToken token);
        Task<Category> GetCategoryById(CancellationToken token, Guid categoryId);
        Task<bool> CreateCategoryAsync(CancellationToken token, Category category);
        Task<bool> DeleteCategory(CancellationToken token, Guid categoryId);
    }
}
