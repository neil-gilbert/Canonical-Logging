using Microsoft.AspNetCore.Http;

namespace CanonicalLogging;

public class CanonicalLoggingMiddleware(RequestDelegate next, CanonicalLoggerProvider canonicalLoggerProvider)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        finally
        {
            var logs = canonicalLoggerProvider.FlushAllLogs();
        }
    }
}