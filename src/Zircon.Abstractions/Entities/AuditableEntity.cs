namespace Zircon.Abstractions.Entities;

public abstract class AuditableEntity
{
    public DateTime CreatedOn { get; private set; }
    public Guid CreatedBy { get; private set; } = Guid.Empty;
    public DateTime? LastModifiedOn { get; private set; }
    public Guid? LastModifiedBy { get; private set; }

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
