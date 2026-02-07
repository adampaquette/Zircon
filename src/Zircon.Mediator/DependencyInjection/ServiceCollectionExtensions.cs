using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Zircon.Mediator.Behaviors;
using Zircon.Mediator.Configuration;
using Zircon.Mediator.Metrics;

namespace Zircon.Mediator.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure Zircon Mediator services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Zircon Mediator services to the service collection, including performance metric behaviors and handlers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">Optional action to configure performance metric options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddZirconMediator(
        this IServiceCollection services,
        Action<PerformanceMetricOptions>? configureOptions = null)
    {
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(HandlerPerformanceMetricBehaviour<,>));
        services.AddSingleton<PerformanceMetricHandler>();

        return services;
    }
}
