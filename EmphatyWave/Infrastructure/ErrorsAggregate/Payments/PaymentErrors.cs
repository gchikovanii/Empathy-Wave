using EmphatyWave.Domain.Localization;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Payments
{
    public static class PaymentErrors
    {
        public static readonly Error PaymentFailed = new("Payment.NotAccepted", ErrorMessages.PaymentFailed);

    }
}
