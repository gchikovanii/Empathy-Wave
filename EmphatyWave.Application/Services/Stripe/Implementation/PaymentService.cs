using EmphatyWave.Application.Services.Stripe.Abstraction;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace EmphatyWave.Application.Services.Stripe.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeClient _stripeClient;
        public PaymentService(IConfiguration configuration)
        {
            var secretKey = configuration["Stripe:SecretKey"];
            _stripeClient = new StripeClient(secretKey);
        }

        public async Task<Charge> ProcessPayment(decimal amount, string currency, string description, string source)
        {
            var options = new ChargeCreateOptions
            {
                Amount = (long)amount,
                Currency = currency,
                Description = description,
                Source = source
            };
            var service = new ChargeService();
            var charge = await service.CreateAsync(options).ConfigureAwait(false);
            return charge;
        }
        public async Task<Charge> GetChargeStatusAsync(string stripeToken) 
        {
            var chargeService = new ChargeService(_stripeClient);
            var charge = await chargeService.GetAsync(stripeToken);
            return charge;
        }
    }
}
