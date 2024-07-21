using EmphatyWave.Domain.Localization;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Products
{
    public static class ProductErrors
    {
        public static readonly Error ProductNotFound = new("Product.NotFound", ErrorMessages.ProductNotFound); 
        public static readonly Error OutOfStock = new("Product.OutOfStock", ErrorMessages.OutOfStock);
        public static readonly Error AlreadyExists = new("Product.AlreadyExists", ErrorMessages.AlreadyExists);

    }
}
