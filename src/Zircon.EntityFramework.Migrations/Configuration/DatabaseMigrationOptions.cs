using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zircon.EntityFramework.Migrations.Abstractions;

namespace Zircon.EntityFramework.Migrations.Configuration;

public class DatabaseMigrationOptions<TContext>
    where TContext : DbContext
{
    internal readonly List<ServiceDescriptor> ServiceOverrides = [];

    private DatabaseMigrationOptions<TContext> Add<TService, TImplementation>(ServiceLifetime lifetime)
        where TService : class
        where TImplementation : class, TService
    {
        ServiceOverrides.Add(new ServiceDescriptor(
            serviceType: typeof(IDataSeeder),
            implementationType: typeof(TImplementation),
            lifetime));

        return this;
    }

    public DatabaseMigrationOptions<TContext> AddScoped<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService => Add<TService, TImplementation>(ServiceLifetime.Scoped);

    public DatabaseMigrationOptions<TContext> AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService => Add<TService, TImplementation>(ServiceLifetime.Transient);

    public DatabaseMigrationOptions<TContext> AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService => Add<TService, TImplementation>(ServiceLifetime.Singleton);

    public DatabaseMigrationOptions<TContext> AddSingleton<TService>(TService implementation)
        where TService : class
    {
        ServiceOverrides.Add(new ServiceDescriptor(typeof(TService), implementation));
        return this;
    }

    public DatabaseMigrationOptions<TContext> AddDataSeeder<TSeeder>()
        where TSeeder : class, IDataSeeder
    {
        AddScoped<IDataSeeder, TSeeder>();
        return this;
    }
}
