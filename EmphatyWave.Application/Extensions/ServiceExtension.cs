using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Helpers;
using EmphatyWave.Application.Jobs;
using EmphatyWave.Application.Services.Account;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.Repositories.Implementation;
using EmphatyWave.Persistence.UOW;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmphatyWave.Application.Extensions
{
    public static class ServiceExtension
    {
        public static void AddServiceExtension(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IAccountService, AccountService>();

            services.AddScoped(typeof(IBaseRepository<>),typeof(BaseRepository<>));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient(provider =>
              new TokenGenerator(config["TokenGenerator:Key"], 5));
            services.AddScoped<JwtProvider>();
            services.AddHostedService<ExpiredPasswordRecoveryTokenWorker>();
            services.AddHostedService<ExpiredVerificationTokenWorker>();
        }
    }
}
