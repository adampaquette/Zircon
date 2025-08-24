using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zircon.EntityFramework.Migrations.Abstractions;
using Zircon.EntityFramework.Migrations.Diagnostics;

namespace Zircon.EntityFramework.Migrations;

public class DatabaseMigrator<TContext>(
    ILogger<DatabaseMigrator<TContext>> logger,
    TContext context) : IDatabaseMigrator<TContext>
    where TContext : DbContext
{
    private readonly ILogger<DatabaseMigrator<TContext>> _logger = logger;
    private readonly TContext _context = context;

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.DataSeederExecutor.StartActivity($"Applying database migrations for {typeof(TContext).Name}");
        try
        {
            _logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await _context.Database.MigrateAsync(cancellationToken);
            });

            _logger.LogInformation("Migration completed for context {DbContextName}", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}",
                typeof(TContext).Name);
            throw;
        }
    }
}
