using EmphatyWave.Domain.Localization;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Categories
{
    public static class CategoryErrors
    {
        public static readonly Error CategoryNotExists = new("Category.NotExists", ErrorMessages.CategoryNotExists);

    }
}
