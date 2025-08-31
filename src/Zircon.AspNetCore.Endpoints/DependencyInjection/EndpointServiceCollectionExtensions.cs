using System.Reflection;

namespace Zircon.AspNetCore.Endpoints.DependencyInjection;

/// <summary>
/// Provides extension methods for registering endpoints with the dependency injection container.
/// </summary>
public static class EndpointServiceCollectionExtensions
{
    /// <summary>
    /// Registers all <see cref="IEndpoint"/> implementations found in the assembly containing the specified type.
    /// This method also invokes each endpoint's ConfigureServices method if implemented.
    /// </summary>
    /// <typeparam name="T">A type from the assembly to scan for endpoints.</typeparam>
    /// <param name="services">The service collection to register endpoints with.</param>
    /// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <example>
    /// <code>
    /// // In Program.cs
    /// builder.Services.AddEndpointsFromAssemblyContaining&lt;Program&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddEndpointsFromAssemblyContaining<T>(this IServiceCollection services)
    {
        var assembly = typeof(T).Assembly;
        return AddEndpointsFromAssembly(services, assembly);
    }

    /// <summary>
    /// Registers all <see cref="IEndpoint"/> implementations found in the specified assembly.
    /// This method automatically discovers all non-abstract classes implementing IEndpoint,
    /// registers them as singletons, and invokes their ConfigureServices method if implemented.
    /// </summary>
    /// <param name="services">The service collection to register endpoints with.</param>
    /// <param name="assembly">The assembly to scan for endpoint implementations.</param>
    /// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <remarks>
    /// Each endpoint is registered as a singleton service implementing IEndpoint.
    /// The ConfigureServices static method on each endpoint is invoked during registration,
    /// allowing endpoints to register their own dependencies in a vertical slice architecture pattern.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs
    /// var assembly = Assembly.GetExecutingAssembly();
    /// builder.Services.AddEndpointsFromAssembly(assembly);
    /// </code>
    /// </example>
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
