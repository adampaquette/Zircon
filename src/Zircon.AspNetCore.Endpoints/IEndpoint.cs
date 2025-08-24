namespace Zircon.AspNetCore.Endpoints;

public interface IEndpoint
{
    RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder);
    static virtual void ConfigureServices(IServiceCollection services) { }
}
