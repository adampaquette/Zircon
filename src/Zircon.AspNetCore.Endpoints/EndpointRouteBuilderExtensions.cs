namespace Zircon.AspNetCore.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    private static bool s_endpointsMapped;
    private static readonly object Lock = new();

    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder endpointRouteBuilder, Action<RouteHandlerBuilder>? configureRouteHandler = null)
    {
        lock (Lock)
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

    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder builder)
        where TEndpoint : IEndpoint, new()
    {
        var endpoint = new TEndpoint();
        endpoint.MapEndpoint(builder);
        return builder;
    }
}
