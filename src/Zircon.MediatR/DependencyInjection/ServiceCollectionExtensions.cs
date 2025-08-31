using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zircon.MediatR.Behaviors;
using Zircon.MediatR.Metrics;

namespace Zircon.MediatR.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure Zircon MediatR services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Zircon MediatR services to the service collection, including performance metric behaviors and handlers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddZirconMediatR(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(HandlerPerformanceMetricBehaviour<,>));
        services.AddScoped<PerformanceMetricHandler>();

        return services;
    }
}
