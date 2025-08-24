using System.Diagnostics;
using MediatR;
using Zircon.MediatR.Metrics;

namespace Zircon.MediatR.Behaviors;

public sealed class HandlerPerformanceMetricBehaviour<TRequest, TResponse>(
    PerformanceMetricHandler performanceMetricHandler)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly PerformanceMetricHandler _performanceMetricHandler =
        performanceMetricHandler;
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next();
        _timer.Stop();

        _performanceMetricHandler.MilliSecondsElapsed(
            _timer.ElapsedMilliseconds);

        return response;
    }
}
