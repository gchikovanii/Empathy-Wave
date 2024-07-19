using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.Localization.Errors;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Categories
{
    public static class CategoryErrors
    {
        public static readonly Error CategoryNotExists = new("Category.NotExists", ErrrorMessages.CategoryNotExists);

    }
}
