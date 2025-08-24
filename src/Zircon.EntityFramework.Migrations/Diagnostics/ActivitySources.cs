using System.Diagnostics;

namespace Zircon.EntityFramework.Migrations.Diagnostics;

internal static class ActivitySources
{
    private const string SourceName = $"{nameof(Zircon)}.{nameof(EntityFramework)}.{nameof(Migrations)}";

    public static readonly ActivitySource DataSeederExecutor =
        new($"{nameof(SourceName)}.{nameof(DataSeederExecutor)}");
}
