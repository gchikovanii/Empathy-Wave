using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Categories;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
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

        public void DeleteCategory(Category category)
        {
            _repo.DeleteData(category);
        }

       
    }
}
