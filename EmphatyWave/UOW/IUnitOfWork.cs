using System.Data;

namespace EmphatyWave.Persistence.UOW
{
    public interface IUnitOfWork
    {
        Task<bool> SaveChangesAsync(CancellationToken token = default);
        Task<IDbTransaction> BeginTransaction(System.Transactions.IsolationLevel level, CancellationToken token = default);
    }
}
