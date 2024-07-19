using EmphatyWave.Domain.Localization;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Orders
{
    public static class OrderErrors
    {
        public static readonly Error OrderNotExists = new("Orders.NotExists", ErrorMessages.OrderNotExists);
        public static readonly Error InaccessibleOrder = new("Orders.Inaccessible", ErrorMessages.InaccessibleOrder);
        public static readonly Error OrderItemsNotExist = new("OrderItems.NotExists", ErrorMessages.OrderItemsNotExists);
    }
}
