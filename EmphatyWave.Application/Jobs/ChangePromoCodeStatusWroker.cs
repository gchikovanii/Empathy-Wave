using EmphatyWave.Application.Services.PromoCodes.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;

namespace EmphatyWave.Application.Jobs
{
    public class ChangePromoCodeStatusWroker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CrontabSchedule _schedule;
        private static string Schedule => "0 */12 * * *";
        private DateTime _nextRun;
        public ChangePromoCodeStatusWroker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _schedule = CrontabSchedule.Parse(Schedule);
            _nextRun = _schedule.GetNextOccurrence(DateTime.UtcNow);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                if (now >= _nextRun)
                {
                    _nextRun = _schedule.GetNextOccurrence(now);
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IPromoCodeService>();
                        await service.ChangeStatusoOfPromoCode(stoppingToken).ConfigureAwait(false);
                    }
                }

                var delay = _nextRun - now;
                if (delay.TotalMilliseconds > 0)
                {
                    await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
                }
            }
        }
    }
}
