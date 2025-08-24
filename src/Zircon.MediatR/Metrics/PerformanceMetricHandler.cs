using System.Diagnostics.Metrics;

namespace Zircon.MediatR.Metrics;

public sealed class PerformanceMetricHandler
{
    private readonly Counter<long> _milliSecondsElapsed;
    public PerformanceMetricHandler(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("VisionFinancIA.Organizations.Api");
        _milliSecondsElapsed = meter.CreateCounter<long>(
            "VisionFinancIA.api.requesthandler.millisecondselapsed");
    }

    public void MilliSecondsElapsed(long milliSecondsElapsed)
        => _milliSecondsElapsed.Add(milliSecondsElapsed);
}
