using EmphatyWave.Application.Services.Account;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;

namespace EmphatyWave.Application.Jobs
{
    public class ExpiredVerificationTokenWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CrontabSchedule _crontabSchedule;
        private static string Schedule => "0 */1 * * *";
        private DateTime _nextRun;

        public ExpiredVerificationTokenWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _crontabSchedule = CrontabSchedule.Parse(Schedule);
            _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                if (now >= _nextRun)
                {
                    _nextRun = _crontabSchedule.GetNextOccurrence(now);
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IAccountService>();
                        await service.RemoveExpiredTokensAsync(stoppingToken, "Verification").ConfigureAwait(false);
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
