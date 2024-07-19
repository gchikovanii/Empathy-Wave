﻿
using EmphatyWave.Domain.Localization;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common
{
    public static class UnitError
    {
        public static readonly Error CantSaveChanges = new("Unit.SaveChangesFailed", ErrorMessages.SaveChanges);
    }
}
