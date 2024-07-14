using EmphatyWave.Domain;
using EmphatyWave.Persistence.DataContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmphatyWave.Application.Extensions
{
    public static class IdentityConfigurationExtension
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentityCore<User>().AddRoles<IdentityRole>().AddEntityFrameworkStores<UserDataContext>().AddApiEndpoints();
            services.AddDbContext<UserDataContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("SqlConnectionString"));
            });
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("SqlConnectionString"));
            });
            return services;
        }
    }
}
