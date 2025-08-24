using MediatR;

namespace Zircon.Abstractions.Events;

/// <summary>
/// Represents a domain event that captures something meaningful that has occurred within the domain.
/// Domain events are immutable and represent past occurrences.
/// Implements INotification to leverage MediatR's publish-subscribe capabilities.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
