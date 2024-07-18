using EmphatyWave.Application.Extensions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace EmphatyWave.Application.Extensions
{
    public static class StripeSettingsExtension
    {
        public static IServiceCollection AddStripeSettings(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<StripeSettings>(config.GetSection("Stripe"));
            var stripeSettings = config.GetSection("Stripe").Get<StripeSettings>();
            StripeConfiguration.ApiKey = stripeSettings.SecretKey;
            return services;
        }
    }
}
