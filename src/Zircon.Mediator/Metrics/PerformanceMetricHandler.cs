using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using Zircon.Mediator.Configuration;

namespace Zircon.Mediator.Metrics;

/// <summary>
/// Handles recording performance metrics for request handler execution.
/// </summary>
public sealed class PerformanceMetricHandler
{
    private readonly Counter<long> _milliSecondsElapsed;

    public PerformanceMetricHandler(IMeterFactory meterFactory, IOptions<PerformanceMetricOptions> options)
    {
        var meterName = options.Value.MeterName;
        var meter = meterFactory.Create(meterName);
        _milliSecondsElapsed = meter.CreateCounter<long>(
            $"{meterName}.requesthandler.millisecondselapsed");
    }

    public void MilliSecondsElapsed(long milliSecondsElapsed)
        => _milliSecondsElapsed.Add(milliSecondsElapsed);
}
