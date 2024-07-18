using Stripe;
using System;

namespace EmphatyWave.Application.Services.Stripe.Abstraction
{
    public interface IPaymentService
    {
        Task<Charge> ProcessPayment(decimal amount, string currency, string description, string source);
        Task<Charge> GetChargeStatusAsync(string stripeToken);
    }
}
