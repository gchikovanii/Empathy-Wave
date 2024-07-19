namespace EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common
{
    public sealed record Error(string code, string? descripton)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static implicit operator Result(Error error) => Result.Failure(error);

    }
}
