using Microsoft.Extensions.Logging;
using Zircon.Diagnostics;
using Zircon.EntityFramework.Migrations.Abstractions;
using Zircon.EntityFramework.Migrations.Diagnostics;

namespace Zircon.EntityFramework.Migrations.Seeding;

public class DataSeederExecutor(
    ILogger<DataSeederExecutor> logger,
    IEnumerable<IDataSeeder> seeders)
    : IDataSeederExecutor
{
    private readonly ILogger<DataSeederExecutor> _logger = logger;
    private readonly IEnumerable<IDataSeeder> _seeders = seeders;

    public async Task ExecuteSeedersAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.DataSeederExecutor.StartActivity();
        foreach (var seeder in _seeders)
        {
            using var seederActivity = ActivitySources.DataSeederExecutor.StartActivity($"Executing seeder {seeder.GetType().Name}");
            try
            {
                _logger.LogInformation("Executing seeder {SeederName}", seeder.GetType().Name);
                await seeder.SeedAsync(cancellationToken);
                _logger.LogInformation("Completed seeder {SeederName}", seeder.GetType().Name);
            }
            catch (Exception ex)
            {
                seederActivity?.SetExceptionTags(ex);
                _logger.LogError(ex, "An error occurred while executing seeder {SeederName}", seeder.GetType().Name);
                throw;
            }
        }
    }
}
