namespace Zircon.Abstractions.Entities;

/// <summary>
/// Interface for entities that support soft deletion functionality.
/// Soft deletion allows entities to be marked as deleted without actually removing them from storage.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets the date and time when the entity was soft deleted.
    /// </summary>
    DateTime? DeletedOn { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who soft deleted the entity.
    /// </summary>
    Guid? DeletedBy { get; set; }
    
    /// <summary>
    /// Gets a value indicating whether the entity is soft deleted.
    /// </summary>
    bool IsDeleted => DeletedOn.HasValue;
}
