using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmphatyWave.Application.Behaviours
{
    public class LoggingPiplineBehaviour<TRequest, TResponse>(ILogger<LoggingPiplineBehaviour<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly ILogger<LoggingPiplineBehaviour<TRequest,TResponse>> _logger = logger;
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var result = await next();
            if (result.IsFailure)
            {
                _logger.LogError("Request Failure: {@RequestName} - {@DateTimeUtc} - {@Error}",
                    typeof(TRequest).Name,DateTime.UtcNow, result.Error);
            }
            return result;
        }
    }
}
