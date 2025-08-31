# Zircon.Abstractions

Core abstractions for domain-driven design including auditable entities, domain events, and soft delete interfaces for Zircon applications.

## Features

- **Auditable Entities**: Automatic tracking of creation and modification timestamps
- **Domain Events**: MediatR-compatible domain event abstractions
- **Soft Delete Support**: Mark entities as deleted without physical removal
- **Domain Event Management**: Track and clear domain events on entities

## Installation

```bash
dotnet add package Zircon.Abstractions
```

## Usage

### Auditable Entities

Create entities that automatically track audit information:

```csharp
using Zircon.Abstractions.Entities;

public class Product : AuditableEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    public void UpdateDetails(string name, decimal price, Guid userId)
    {
        Name = name;
        Price = price;
        SetAuditFields(userId, isCreate: false);
    }
}

// Usage
var product = new Product();
product.SetAuditFields(userId, isCreate: true); // Sets CreatedOn and CreatedBy
```

### Soft Deletable Entities

Implement soft deletion functionality:

```csharp
using Zircon.Abstractions.Entities;

public class Customer : AuditableEntity, ISoftDeletable
{
    public string Name { get; set; }
    public DateTime? DeletedOn { get; set; }
    public Guid? DeletedBy { get; set; }
    
    public void SoftDelete(Guid userId)
    {
        DeletedOn = DateTime.UtcNow;
        DeletedBy = userId;
    }
}

// Usage
var customer = new Customer { Name = "John Doe" };
if (!customer.IsDeleted)
{
    customer.SoftDelete(userId);
}
```

### Domain Events

Create and manage domain events:

```csharp
using MediatR;
using Zircon.Abstractions.Events;

public class OrderCreatedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; }
    public string OrderId { get; }
    
    public OrderCreatedEvent(string orderId)
    {
        OrderId = orderId;
        OccurredOn = DateTime.UtcNow;
    }
}

public class Order : AuditableEntity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public string OrderNumber { get; set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void CompleteOrder()
    {
        // Business logic here
        _domainEvents.Add(new OrderCreatedEvent(OrderNumber));
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

### Entity with All Features

Combine all abstractions in a single entity:

```csharp
public class Invoice : AuditableEntity, ISoftDeletable, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public string InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    
    // ISoftDeletable implementation
    public DateTime? DeletedOn { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // IHasDomainEvents implementation
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    // Business methods
    public void Approve(Guid userId)
    {
        SetAuditFields(userId, isCreate: false);
        _domainEvents.Add(new InvoiceApprovedEvent(InvoiceNumber));
    }
}
```

## Key Interfaces and Classes

### AuditableEntity
Base class providing automatic audit field tracking:
- `CreatedOn`: When the entity was created
- `CreatedBy`: User who created the entity  
- `LastModifiedOn`: When the entity was last modified
- `LastModifiedBy`: User who last modified the entity
- `SetAuditFields(userId, isCreate)`: Updates audit fields

### ISoftDeletable
Interface for entities supporting soft deletion:
- `DeletedOn`: When the entity was soft deleted
- `DeletedBy`: User who soft deleted the entity
- `IsDeleted`: Computed property indicating if entity is deleted

### IDomainEvent
Interface for domain events (extends MediatR.INotification):
- `OccurredOn`: When the domain event occurred

### IHasDomainEvents
Interface for entities that can raise domain events:
- `DomainEvents`: Collection of raised domain events
- `ClearDomainEvents()`: Clears all domain events

## Integration with Entity Framework

These abstractions work seamlessly with Entity Framework Core:

```csharp
public class ApplicationDbContext : DbContext
{
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }
    
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetAuditFields(GetCurrentUserId(), true);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetAuditFields(GetCurrentUserId(), false);
            }
        }
    }
}
```

## Best Practices

1. **Combine Interfaces**: Most domain entities benefit from implementing multiple abstractions
2. **Domain Events**: Use domain events to decouple business logic and side effects
3. **Audit Tracking**: Always call `SetAuditFields` when modifying entities
4. **Soft Delete Queries**: Remember to filter out soft-deleted entities in queries
5. **Event Clearing**: Clear domain events after publishing them to avoid memory leaks

## License

This project is licensed under the MIT License - see the LICENSE file for details.