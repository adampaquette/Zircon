namespace Zircon.AspNetCore.Endpoints;

/// <summary>
/// Defines a contract for implementing minimal API endpoints in a vertical slice architecture pattern.
/// Each endpoint implementation encapsulates its routing, handling logic, and optional service configuration.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint route and handler to the application's endpoint route builder.
    /// This method defines the HTTP route, verb, handler logic, and any endpoint-specific configuration.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder to register the endpoint with.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be further configured.</returns>
    RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder);
    
    /// <summary>
    /// Optionally configures services specific to this endpoint during application startup.
    /// This static method is called during service registration and allows each endpoint to register
    /// its own dependencies, promoting independence in a vertical slice architecture.
    /// </summary>
    /// <param name="services">The service collection to register dependencies with.</param>
    static virtual void ConfigureServices(IServiceCollection services) { }
}
