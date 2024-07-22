using EmphatyWave.Application.Queries.Categories;
using EmphatyWave.Persistence.Repositories.Abstraction;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmphatyWave.ApiService.Infrastructure.HealtChecks
{
    public class TimeIntervalOfProcessingOrderHelthCheck(IMediator mediator) : IHealthCheck
    {
        private readonly IMediator _mediator = mediator;
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
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
            catch (Exception ex)
            {
                stopwatch.Stop();
                return HealthCheckResult.Unhealthy($"Exception occurred: {ex.Message}");
            }
        }
    }
}
