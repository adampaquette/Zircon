namespace Zircon.EntityFramework.Migrations.Abstractions;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
