using EmphatyWave.Domain.Localization;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.PromoCodeErrors
{
    public class PromoCodeErrors
    {
        public static readonly Error PromoCodeNotFound = new("PromoCode.NotFound", ErrorMessages.PromoNotFound);
        public static readonly Error PromoCodeAlreadyExists = new("PromoCode.AlreadyExists", ErrorMessages.PromoAlreadyExists);
    }
}
