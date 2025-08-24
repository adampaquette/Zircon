using Microsoft.AspNetCore.Builder;

namespace Zircon.AspNetCore.Middlewares;

public static class DeveloperRequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseDeveloperRequestLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<DeveloperRequestLoggingMiddleware>();
    }
}
