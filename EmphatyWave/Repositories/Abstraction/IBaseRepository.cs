using System.Linq.Expressions;

namespace EmphatyWave.Persistence.Repositories.Abstraction
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> GetQuery(Expression<Func<T, bool>> expression = null);
        Task<ICollection<T>> GetPaginatedData(CancellationToken token,int pageNumber, int pageSize);
        Task<T> GetDataById(CancellationToken token,Guid id);
        Task CreateData(CancellationToken token, T entity);
        void UpdateData(T entity);
        void DeleteData(T entity);
    }
}
