using EmphatyWave.Persistence.Infrastructure.Localization.Errors;

namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common
{
    public class Result
    {
        public Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            {
                throw new ArgumentException($"{ErrrorMessages.InvalidError} - {nameof(error)}");
            }
            IsSuccess = isSuccess;
            Error = error;
        }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }
        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);
    }
}
