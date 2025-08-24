using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zircon.EntityFramework.Migrations.Abstractions;

namespace Zircon.EntityFramework.Migrations.DependencyInjection;

internal class MigrationServiceProviderFactory<TContext>(
    IServiceCollection serviceCollection,
    IEnumerable<ServiceDescriptor> serviceOverrides)
    : IMigrationServiceProviderFactory<TContext> where TContext : DbContext
{
    private readonly IServiceCollection _serviceCollection = serviceCollection;
    private readonly IEnumerable<ServiceDescriptor> _serviceOverrides = serviceOverrides;

    public IServiceProvider Create()
    {
        var services = new ServiceCollection();
        foreach (var descriptor in _serviceCollection)
        {
            services.Add(descriptor);
        }

        foreach (var serviceOverride in _serviceOverrides)
        {
            services.RemoveAll(serviceOverride.ServiceType);
        }

        foreach (var serviceOverride in _serviceOverrides)
        {
            services.Add(serviceOverride);
        }

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }
}
