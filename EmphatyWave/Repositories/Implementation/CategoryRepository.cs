using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class CategoryRepository(IBaseRepository<Category> repo, IUnitOfWork unit) : ICategoryRepository
    {
        private readonly IBaseRepository<Category> _repo = repo;
        private readonly IUnitOfWork _unit = unit;

        public async Task<ICollection<Category>> GetCategories(CancellationToken token)
        {
            return await _repo.GetQuery().ToListAsync(token).ConfigureAwait(false);
        }
        public async Task<Category> GetCategoryById(CancellationToken token, Guid categoryId)
        {
            return await _repo.GetDataById(token, categoryId).ConfigureAwait(false);
        }
        public async Task<bool> CreateCategoryAsync(CancellationToken token, Category category)
        {
            await _repo.CreateData(token, category);
            return await _unit.SaveChangesAsync(token).ConfigureAwait(false);
        }

        public async Task<bool> DeleteCategory(CancellationToken token, Guid categoryId)
        {
            var category = await _repo.GetDataById(token, categoryId);
            if (category == null)
                throw new Exception("Category Doesnt exists with this id");
            _repo.DeleteData(category);
            return await _unit.SaveChangesAsync(token).ConfigureAwait(false);
        }

       
    }
}
