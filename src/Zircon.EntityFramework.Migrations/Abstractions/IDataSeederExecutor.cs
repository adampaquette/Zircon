namespace Zircon.EntityFramework.Migrations.Abstractions;

public interface IDataSeederExecutor
{
    Task ExecuteSeedersAsync(CancellationToken cancellationToken = default);
}
