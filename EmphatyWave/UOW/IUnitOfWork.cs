namespace EmphatyWave.Persistence.UOW
{
    public interface IUnitOfWork
    {
        Task<bool> SaveChangesAsync(CancellationToken token = default);
    }
}
