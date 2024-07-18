using EmphatyWave.Application.Services.Stripe.Abstraction;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;

namespace EmphatyWave.Application.Jobs
{
    public class PendingPaymentWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CrontabSchedule _schedule;
        private static string Schedule => "0 */5 * * *"; // Every 5 minutes
        private DateTime _nextRun;

        public PendingPaymentWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _schedule = CrontabSchedule.Parse(Schedule);
            _nextRun = _schedule.GetNextOccurrence(DateTime.UtcNow);
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                if (now >= _nextRun)
                {
                    _nextRun = _schedule.GetNextOccurrence(_nextRun);
                    using(var scope = _scopeFactory.CreateScope())
                    {
                        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                        var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var pendingOrders = await orderRepository.GetPendingOrdersAsync(stoppingToken).ConfigureAwait(false);
                        foreach (var order in pendingOrders)
                        {
                            var charge = await paymentService.GetChargeStatusAsync(order.StripeToken).ConfigureAwait(false);
                            if (charge.Status == "succeeded")
                            {
                                order.Status = Status.PaymentSucceeded;
                                order.StripeToken = null;
                            }
                            else if (charge.Status == "failed")
                            {
                                order.Status = Status.PaymentFailed;
                                //Manually RollBack
                                var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                                var orderItemsRepo = scope.ServiceProvider.GetRequiredService<IOrderItemRepository>();
                               
                                var orderItems = await orderItemsRepo.GetOrderItems(stoppingToken,order.Id).ConfigureAwait(false);
                                foreach (var orderItem in orderItems)
                                {
                                    var currentProduct = await productRepo.GetProductById(stoppingToken, orderItem.ProductId);
                                    currentProduct.Quantity += orderItem.Quantity;
                                    productRepo.UpdateProduct(currentProduct);
                                }
                            }
                            orderRepository.UpdateOrder(order);
                            await unit.SaveChangesAsync(stoppingToken).ConfigureAwait(false);

                        }
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
