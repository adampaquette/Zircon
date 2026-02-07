namespace Zircon.Mediator.Configuration;

/// <summary>
/// Configuration options for performance metric collection.
/// </summary>
public sealed class PerformanceMetricOptions
{
    /// <summary>
    /// Gets or sets the name of the meter used for performance metrics.
    /// </summary>
    public string MeterName { get; set; } = "Zircon";
}
