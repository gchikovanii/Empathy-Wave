using EmphatyWave.Domain.Localization;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.PromoCodeErrors
{
    public class PromoCodeErrors
    {
        
        public static readonly Error PromoCodeNotFound = new("PromoCode.NotFound", ErrorMessages.PromoNotFound); 
        public static readonly Error PromoCodeAlreadyExists = new("PromoCode.AlreadyExists", ErrorMessages.PromoAlreadyExists);
        public static readonly Error PromoCodeError = new("PromoCode.PromoCodeError", ErrorMessages.PromoCodeError);
        public static readonly Error PromoCodeIncorrectTimeError = new("PromoCodeIncor.PromoCodeIncorrectTimeError", ErrorMessages.PromoCodeIncorrectTimeError);
    }
}
