using EmphatyWave.Persistence.DataContext;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace EmphatyWave.Persistence.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IDbTransaction> BeginTransaction(IsolationLevel level, CancellationToken token = default)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            return transaction.GetDbTransaction();
        }

        public async Task<bool> SaveChangesAsync(CancellationToken token = default)
        {
            return await _context.SaveChangesAsync(token).ConfigureAwait(false) > 0;
        }
    }
}
