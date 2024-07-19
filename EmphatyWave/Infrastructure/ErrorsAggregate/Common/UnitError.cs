using EmphatyWave.Persistence.Infrastructure.Localization.Errors;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common
{
    public static class UnitError
    {
        public static readonly Error CantSaveChanges = new("Unit.SaveChangesFailed", ErrrorMessages.SaveChanges);
    }
}
