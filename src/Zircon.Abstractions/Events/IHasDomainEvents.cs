namespace Zircon.Abstractions.Events;

/// <summary>
/// Interface for entities that can raise and manage domain events.
/// Provides functionality for tracking domain events that occur within an entity's lifecycle.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the collection of domain events that have been raised by this entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    
    /// <summary>
    /// Clears all domain events from the entity.
    /// This is typically called after events have been published.
    /// </summary>
    void ClearDomainEvents();
}
