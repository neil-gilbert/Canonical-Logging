using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CanonicalLogging;

public static class CanonicalLoggingExtensions
{
    public static IServiceCollection AddCanonicalLogging(this IServiceCollection services)
    {
        services.AddSingleton<CanonicalLoggerProvider>(sp => 
            new CanonicalLoggerProvider(sp.GetRequiredService<ILoggerFactory>()));
        return services;
    }

    public static IApplicationBuilder UseCanonicalLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CanonicalLoggingMiddleware>();
    }
}