using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zircon.EntityFramework.Migrations.Abstractions;
using Zircon.EntityFramework.Migrations.Diagnostics;

namespace Zircon.EntityFramework.Migrations.Hosting;

public class DatabaseInitializationHostedService<TContext>(
    IMigrationServiceProviderFactory<TContext> migrationServiceProviderFactory,
    ILogger<DatabaseInitializationHostedService<TContext>> logger)
    : IHostedService
    where TContext : DbContext
{
    private readonly IMigrationServiceProviderFactory<TContext> _migrationServiceProviderFactory = migrationServiceProviderFactory;
    private readonly ILogger<DatabaseInitializationHostedService<TContext>> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.DataSeederExecutor.StartActivity($"Database initialization for {typeof(TContext).Name}");
        try
        {
            _logger.LogInformation("Starting database initialization");

            var migrationServiceProvider = _migrationServiceProviderFactory.Create();
            using var scope = migrationServiceProvider.CreateScope();

            var migrator = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator<TContext>>();
            await migrator.MigrateAsync(cancellationToken);

            var seederExecutor = scope.ServiceProvider.GetRequiredService<IDataSeederExecutor>();
            await seederExecutor.ExecuteSeedersAsync(cancellationToken);

            _logger.LogInformation("Database initialization completed");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
