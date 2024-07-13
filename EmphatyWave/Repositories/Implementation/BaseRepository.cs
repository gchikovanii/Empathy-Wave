using EmphatyWave.Domain;
using EmphatyWave.Persistence.DataContext;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public IQueryable<T> GetQuery(Expression<Func<T, bool>> expression = null)
        {
            return expression == null ? _context.Set<T>() : _context.Set<T>().AsNoTracking().Where(expression);
        }
        public async Task<ICollection<T>> GetPaginatedData(CancellationToken token,int pageNumber, int pageSize)
        {
            return await _context.Set<T>().AsNoTracking().Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(token).ConfigureAwait(false);
        }
        public async Task<T> GetDataById(CancellationToken token, Guid id)
        {
            return await _context.Set<T>().FindAsync(id,token).ConfigureAwait(false);
        }

        public async Task CreateData(CancellationToken token,T entity)
        {
            await _context.AddAsync(entity,token).ConfigureAwait(false);
        }
        public void UpdateData(T entity)
        {
            _context.Update(entity);
        }
        public void DeleteData(T entity)
        {
            _context.Remove(entity);
        }
    }
}
