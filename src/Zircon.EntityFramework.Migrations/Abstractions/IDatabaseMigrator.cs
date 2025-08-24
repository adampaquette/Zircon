using Microsoft.EntityFrameworkCore;

namespace Zircon.EntityFramework.Migrations.Abstractions;

public interface IDatabaseMigrator<TContext>
    where TContext : DbContext
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
