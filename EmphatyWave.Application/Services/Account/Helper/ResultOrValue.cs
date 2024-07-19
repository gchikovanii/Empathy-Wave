using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;

namespace EmphatyWave.Application.Services.Account.Helper
{
    public class ResultOrValue<T>
    {
        public T Value { get; }
        public Result Result { get; }
        public bool IsSuccess { get; }

        private ResultOrValue(T value)
        {
            Value = value;
            IsSuccess = true;
        }

        private ResultOrValue(Result result)
        {
            Result = result;
            IsSuccess = false;
        }

        public static ResultOrValue<T> Success(T value) => new ResultOrValue<T>(value);
        public static ResultOrValue<T> Failure(Result result) => new ResultOrValue<T>(result);
    }
}
