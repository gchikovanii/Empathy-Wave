using EmphatyWave.ApiService.Infrastructure.Middlewares;

namespace EmphatyWave.ApiService.Infrastructure.Extensions
{
    public static class CultureMiddlewareExtension
    {
        public static IApplicationBuilder UseCulture(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CultureConfigurationMiddleware>();
        }
    }
}
