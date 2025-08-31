using Microsoft.Extensions.Configuration;

namespace Zircon.Configuration;

/// <summary>
/// Extension methods for <see cref="IConfiguration"/> to provide enhanced configuration access.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a required configuration value, throwing an exception if the value is not found.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="name">The name of the configuration key.</param>
    /// <returns>The configuration value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the configuration value is missing.</exception>
    public static string GetRequiredValue(this IConfiguration configuration, string name) =>
        configuration[name] ?? throw new InvalidOperationException(
            $"Configuration missing value for: {(configuration is IConfigurationSection s ? s.Path + ":" + name : name)}");
}
