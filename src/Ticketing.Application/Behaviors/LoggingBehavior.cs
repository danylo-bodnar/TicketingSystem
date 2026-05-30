using System.Diagnostics;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ticketing.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogDebug("Processing {RequestType}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            if (response is not null && response is not Unit)
            {
                var responseType = response.GetType();
                if (responseType.IsGenericType &&
                    responseType.GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var isSuccess = (bool)responseType.GetProperty("IsSuccess")!.GetValue(response)!;
                    if (!isSuccess)
                    {
                        var error = (string?)responseType.GetProperty("Error")?.GetValue(response);
                        _logger.LogWarning(
                            "Completed {RequestType} in {Elapsed}ms — Failed: {Error}",
                            requestName, stopwatch.ElapsedMilliseconds, error);
                        return response;
                    }
                }
            }

            _logger.LogInformation(
                "Completed {RequestType} in {Elapsed}ms",
                requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Exception in {RequestType} after {Elapsed}ms: {ErrorMessage}",
                requestName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}
