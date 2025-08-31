namespace Zircon.Abstractions.Entities;

/// <summary>
/// Base class for entities that require audit tracking functionality.
/// Provides automatic tracking of creation and modification timestamps and user IDs.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedOn { get; private set; }
    
    /// <summary>
    /// Gets the ID of the user who created the entity.
    /// </summary>
    public Guid CreatedBy { get; private set; } = Guid.Empty;
    
    /// <summary>
    /// Gets the date and time when the entity was last modified.
    /// </summary>
    public DateTime? LastModifiedOn { get; private set; }
    
    /// <summary>
    /// Gets the ID of the user who last modified the entity.
    /// </summary>
    public Guid? LastModifiedBy { get; private set; }

    /// <summary>
    /// Sets the audit fields for the entity based on whether this is a create or update operation.
    /// </summary>
    /// <param name="userId">The ID of the user performing the operation.</param>
    /// <param name="isCreate">True if this is a create operation; false if it's an update operation.</param>
    public void SetAuditFields(Guid userId, bool isCreate)
    {
        if (isCreate)
        {
            CreatedOn = DateTime.UtcNow;
            CreatedBy = userId;
        }

        LastModifiedOn = DateTime.UtcNow;
        LastModifiedBy = userId;
    }
}
