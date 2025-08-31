using System.Diagnostics;

namespace Zircon.Diagnostics;

/// <summary>
/// Extension methods for <see cref="Activity"/> to enhance diagnostic capabilities.
/// </summary>
public static class ActivityExtensions
{
    /// <summary>
    /// Sets exception-related tags on an activity following OpenTelemetry semantic conventions.
    /// Adds exception message, stack trace, type, and sets the activity status to Error.
    /// </summary>
    /// <param name="activity">The activity to set tags on. If null, no action is taken.</param>
    /// <param name="ex">The exception to extract information from.</param>
    /// <remarks>
    /// See <see href="https://opentelemetry.io/docs/specs/otel/trace/semantic_conventions/exceptions/">OpenTelemetry Exception Semantic Conventions</see>
    /// </remarks>
    public static void SetExceptionTags(this Activity? activity, Exception ex)
    {
        if (activity is null)
        {
            return;
        }

        activity.AddTag("exception.message", ex.Message);
        activity.AddTag("exception.stacktrace", ex.ToString());
        activity.AddTag("exception.type", ex.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
