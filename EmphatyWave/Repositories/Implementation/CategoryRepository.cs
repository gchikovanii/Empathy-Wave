using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class CategoryRepository(IBaseRepository<Category> repo) : ICategoryRepository
    {
        private readonly IBaseRepository<Category> _repo = repo;

        public async Task<ICollection<Category>> GetCategories(CancellationToken token)
        {
            return await _repo.GetQuery().ToListAsync(token).ConfigureAwait(false);
        }
        public async Task<Category> GetCategoryById(CancellationToken token, Guid categoryId)
        {
            return await _repo.GetDataById(token, categoryId).ConfigureAwait(false);
        }
        public async Task CreateCategoryAsync(CancellationToken token, Category category)
        {
            category.Id = Guid.NewGuid();
            await _repo.CreateData(token, category);
        }

        public async Task DeleteCategory(CancellationToken token, Guid categoryId)
        {
            var category = await _repo.GetDataById(token, categoryId);
            if (category == null)
                throw new Exception("Category Doesnt exists with this id");
            _repo.DeleteData(category);
        }

       
    }
}
