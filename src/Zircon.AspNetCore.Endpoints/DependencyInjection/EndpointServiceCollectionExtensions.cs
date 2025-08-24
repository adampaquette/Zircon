using System.Reflection;

namespace Zircon.AspNetCore.Endpoints.DependencyInjection;

public static class EndpointServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointsFromAssemblyContaining<T>(this IServiceCollection services)
    {
        var assembly = typeof(T).Assembly;
        return AddEndpointsFromAssembly(services, assembly);
    }

    public static IServiceCollection AddEndpointsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var endpointTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IEndpoint).IsAssignableFrom(t));

        services.RegisterTypes(endpointTypes);

        return services;
    }

    private static void RegisterTypes(this IServiceCollection services, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            services.AddSingleton(typeof(IEndpoint), type);

            var configureServicesMethod = type.GetMethod(
                nameof(IEndpoint.ConfigureServices),
                BindingFlags.Public | BindingFlags.Static);

            configureServicesMethod?.Invoke(null, [services]);
        }
    }
}
