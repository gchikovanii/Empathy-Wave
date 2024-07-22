using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmphatyWave.ApiService.Infrastructure.HealtChecks
{
    public class TimeIntervalOfFetchingCategoriesHelthCheck(ICategoryRepository categoryRepository) : IHealthCheck
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await _categoryRepository.GetCategories(cancellationToken);
            stopwatch.Stop();

            var responseTime = stopwatch.ElapsedMilliseconds;

            if (responseTime < 500)
            {
                return HealthCheckResult.Healthy($"Response time is {responseTime}ms");
            }
            else if (responseTime < 1000)
            {
                return HealthCheckResult.Degraded($"Response time is {responseTime}ms");
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Response time is {responseTime}ms");
            }
        }
    }
}
