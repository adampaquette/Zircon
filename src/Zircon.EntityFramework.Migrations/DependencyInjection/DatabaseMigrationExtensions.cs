using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zircon.EntityFramework.Migrations.Abstractions;
using Zircon.EntityFramework.Migrations.Configuration;
using Zircon.EntityFramework.Migrations.Diagnostics;
using Zircon.EntityFramework.Migrations.Hosting;
using Zircon.EntityFramework.Migrations.Seeding;

namespace Zircon.EntityFramework.Migrations.DependencyInjection;

public static class DatabaseMigrationExtensions
{
    public static IServiceCollection AddDatabaseMigrations<TContext>(
        this IServiceCollection services,
        Action<DatabaseMigrationOptions<TContext>> configure)
        where TContext : DbContext
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(ActivitySources.DataSeederExecutor.Name));

        AddDefaultServiceImplementations<TContext>(services);

        var options = new DatabaseMigrationOptions<TContext>();
        configure(options);

        services.AddSingleton<IMigrationServiceProviderFactory<TContext>>(
            new MigrationServiceProviderFactory<TContext>(services, options.ServiceOverrides));

        services.AddHostedService<DatabaseInitializationHostedService<TContext>>();

        return services;
    }

    private static void AddDefaultServiceImplementations<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<IDatabaseMigrator<TContext>, DatabaseMigrator<TContext>>();
        services.AddScoped<IDataSeederExecutor, DataSeederExecutor>();
    }
}
