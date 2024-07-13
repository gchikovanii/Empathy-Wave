using EmphatyWave.Persistence.DataContext;

namespace EmphatyWave.Persistence.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken token = default)
        {
            return await _context.SaveChangesAsync(token).ConfigureAwait(false) > 0;
        }
    }
}
