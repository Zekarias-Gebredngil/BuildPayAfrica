using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildPay.ServiceDefaults;

public static class Correlation
{
    public const string HeaderName = "X-Correlation-Id";
    public const string LogPropertyName = "correlationId";
}

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers[Correlation.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[Correlation.HeaderName] = correlationId;
        }

        context.Response.Headers[Correlation.HeaderName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object?>
               {
                   [Correlation.LogPropertyName] = correlationId
               }))
        {
            await next(context);
        }
    }
}

