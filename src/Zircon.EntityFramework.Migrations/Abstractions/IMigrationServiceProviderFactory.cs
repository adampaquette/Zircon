using Microsoft.EntityFrameworkCore;

namespace Zircon.EntityFramework.Migrations.Abstractions;

public interface IMigrationServiceProviderFactory<TContext> where TContext : DbContext
{
    IServiceProvider Create();
}
