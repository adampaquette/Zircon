using Zircon.AspNetCore.Endpoints.DependencyInjection;

namespace Zircon.AspNetCore.Endpoints;

/// <summary>
/// Provides extension methods for mapping endpoints to the ASP.NET Core routing system.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    private static bool s_endpointsMapped;
    private static readonly Lock s_lock = new();

    /// <summary>
    /// Maps all registered <see cref="IEndpoint"/> implementations from the dependency injection container.
    /// This method should be called once during application startup to register all endpoints that were
    /// added using the <see cref="EndpointServiceCollectionExtensions.AddEndpointsFromAssembly"/> extension methods.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder to map endpoints to.</param>
    /// <param name="configureRouteHandler">An optional action to configure each route handler after mapping.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for method chaining.</returns>
    /// <remarks>
    /// This method is thread-safe and ensures endpoints are only mapped once per application lifetime.
    /// All endpoints registered in the DI container as IEndpoint will be automatically mapped.
    /// </remarks>
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder endpointRouteBuilder, Action<RouteHandlerBuilder>? configureRouteHandler = null)
    {
        lock (s_lock)
        {
            if (s_endpointsMapped) return endpointRouteBuilder;

            foreach (var endpoint in endpointRouteBuilder.ServiceProvider.GetServices<IEndpoint>())
            {
                var routeHandlerBuilder = endpoint.MapEndpoint(endpointRouteBuilder);
                configureRouteHandler?.Invoke(routeHandlerBuilder);
            }
            s_endpointsMapped = true;
        }

        return endpointRouteBuilder;
    }

    /// <summary>
    /// Maps a single endpoint of the specified type directly without requiring dependency injection registration.
    /// Useful for testing or when you want to map specific endpoints manually.
    /// </summary>
    /// <typeparam name="TEndpoint">The type of endpoint to map. Must have a parameterless constructor.</typeparam>
    /// <param name="builder">The endpoint route builder to map the endpoint to.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for method chaining.</returns>
    /// <example>
    /// <code>
    /// app.MapEndpoint&lt;CreateProductEndpoint&gt;();
    /// app.MapEndpoint&lt;GetProductsEndpoint&gt;();
    /// </code>
    /// </example>
    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder builder)
        where TEndpoint : IEndpoint, new()
    {
        var endpoint = new TEndpoint();
        endpoint.MapEndpoint(builder);
        return builder;
    }
}
