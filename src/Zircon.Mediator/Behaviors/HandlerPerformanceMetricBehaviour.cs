using System.Diagnostics;
using Mediator;
using Zircon.Mediator.Metrics;

namespace Zircon.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that measures the execution time of request handlers.
/// </summary>
public sealed class HandlerPerformanceMetricBehaviour<TMessage, TResponse>(
    PerformanceMetricHandler performanceMetricHandler)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : notnull, IMessage
{
    private readonly Stopwatch _timer = new();

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next(message, cancellationToken);
        _timer.Stop();

        performanceMetricHandler.MilliSecondsElapsed(_timer.ElapsedMilliseconds);

        return response;
    }
}
