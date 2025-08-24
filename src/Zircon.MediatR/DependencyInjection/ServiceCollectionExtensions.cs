using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zircon.MediatR.Behaviors;
using Zircon.MediatR.Metrics;

namespace Zircon.MediatR.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZirconMediatR(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(HandlerPerformanceMetricBehaviour<,>));
        services.AddScoped<PerformanceMetricHandler>();

        return services;
    }
}
