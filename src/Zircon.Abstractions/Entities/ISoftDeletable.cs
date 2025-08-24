namespace Zircon.Abstractions.Entities;

public interface ISoftDeletable
{
    DateTime? DeletedOn { get; set; }
    Guid? DeletedBy { get; set; }
    bool IsDeleted => DeletedOn.HasValue;
}
