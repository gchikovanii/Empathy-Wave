using EmphatyWave.Domain;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface ICategoryRepository
    {
        Task<ICollection<Category>> GetCategories(CancellationToken token);
        Task<Category> GetCategoryById(CancellationToken token, Guid categoryId);
        Task<Category> GetCategoryByName(CancellationToken token, string categoryName);
        Task CreateCategoryAsync(CancellationToken token, Category category);
        void DeleteCategory(Category category); 
    }
}
