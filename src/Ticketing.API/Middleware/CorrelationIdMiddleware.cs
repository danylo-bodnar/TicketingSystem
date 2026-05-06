using System.Diagnostics;
using Serilog.Context;
using Ticketing.Application.Common;

public class CorrelationIdMiddleware
{
    private const string Header = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers[Header].FirstOrDefault()
            ?? Activity.Current?.TraceId.ToString()
            ?? Guid.NewGuid().ToString();

        CorrelationContext.Set(correlationId);

        context.Response.Headers[Header] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}