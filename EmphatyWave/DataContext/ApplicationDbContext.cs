using EmphatyWave.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.DataContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().Property(i => i.Status).HasConversion<string>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
