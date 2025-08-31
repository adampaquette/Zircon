# Zircon NuGet Packages

## Overview

The Zircon library collection provides a comprehensive set of utilities and patterns for building robust .NET applications. All packages follow consistent standards for documentation, testing, and maintainability.

## Available Packages

### Core Packages

| Package | Version | Description |
|---------|---------|-------------|
| **Zircon.Results** | 1.0.0 | Result pattern implementation for clean error handling with customizable error extraction |
| **Zircon.Abstractions** | 1.0.0 | Core domain-driven design abstractions including auditable entities and domain events |
| **Zircon.Configuration** | 1.0.0 | Enhanced configuration extensions with required value validation |
| **Zircon.Security.Claims.Extensions** | 1.0.0 | ClaimsPrincipal authentication utilities for safer security operations |

### Development & Testing

| Package | Version | Description |
|---------|---------|-------------|
| **Zircon.Test** | 1.0.0 | Testing utilities with AutoFixture and AutoMoq integration for streamlined unit testing |
| **Zircon.Diagnostics** | 1.0.0 | OpenTelemetry activity extensions for standardized exception tracking |

### Infrastructure

| Package | Version | Description |
|---------|---------|-------------|
| **Zircon.IO** | 1.0.0 | Stream extension methods for efficient async I/O operations |
| **Zircon.Serialization** | 1.0.0 | JSON converters for System.Text.Json with culture-aware decimal handling |
| **Zircon.MediatR** | 1.0.0 | MediatR pipeline behaviors with built-in performance monitoring |
| **Zircon.AspNetCore.Endpoints** | 1.0.0 | Vertical slice architecture support for ASP.NET Core Minimal APIs |

## Installation

Install any package using the .NET CLI:

```bash
dotnet add package Zircon.Results
dotnet add package Zircon.Abstractions
dotnet add package Zircon.Configuration
# ... etc
```

Or via Package Manager Console:

```powershell
Install-Package Zircon.Results
Install-Package Zircon.Abstractions
Install-Package Zircon.Configuration
# ... etc
```

## Quick Start Examples

### Result Pattern (Zircon.Results)

```csharp
public Result<Product> GetProduct(int id)
{
    if (id <= 0)
        return Result<Product>.Failure("Invalid product ID");
    
    var product = _repository.FindById(id);
    return product != null 
        ? Result<Product>.Success(product)
        : Result<Product>.Failure("Product not found");
}
```

### Domain Abstractions (Zircon.Abstractions)

```csharp
public class Order : AuditableEntity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public void PlaceOrder()
    {
        // Business logic
        _domainEvents.Add(new OrderPlacedEvent(Id));
    }
    
    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents;
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Configuration (Zircon.Configuration)

```csharp
// Throws InvalidOperationException if missing
var apiKey = configuration.GetRequiredValue("ApiKey");
var connectionString = configuration.GetRequiredValue("ConnectionStrings:Default");
```

### Testing (Zircon.Test)

```csharp
public class ProductServiceFixture : AutoMoqFixture<ProductService>
{
    public Mock<IProductRepository> RepositoryMock => GetMock<IProductRepository>();
    
    public ProductServiceFixture WithProduct(Product product)
    {
        RepositoryMock.Setup(x => x.GetByIdAsync(product.Id))
                      .ReturnsAsync(product);
        return this;
    }
}
```

## Package Features

### Zircon.Results
- Generic and non-generic Result types
- Exception handling with IExceptionErrorExtractor
- Task extensions for async operations
- Pluggable error extraction system

### Zircon.Abstractions
- AuditableEntity base class with automatic timestamp management
- ISoftDeletable interface for soft delete patterns
- Domain event interfaces (IDomainEvent, IHasDomainEvents)
- Integration with MediatR for event handling

### Zircon.Configuration
- GetRequiredValue extension method
- Detailed error messages with configuration path
- Null-safe operations

### Zircon.Security.Claims.Extensions
- IsAuthenticated extension method
- Null-safe ClaimsPrincipal operations
- Simplified authentication checks

### Zircon.Test
- AutoMoqFixture base class for test setup
- Lazy SUT (System Under Test) creation
- Mock freezing and retrieval
- Integration with AutoFixture and Moq

### Zircon.Diagnostics
- SetExceptionTags extension for Activity
- OpenTelemetry semantic conventions
- Standardized exception tracking

### Zircon.IO
- ReadAllBytesAsync for efficient stream reading
- Memory-efficient buffer management
- Async/await support

### Zircon.Serialization
- DecimalConverter for culture-aware decimal handling
- Support for both string and number formats
- Integration with System.Text.Json

### Zircon.MediatR
- Performance monitoring pipeline behavior
- Validation pipeline with FluentValidation
- Metrics collection for request handling
- Dependency injection extensions

### Zircon.AspNetCore.Endpoints
- IEndpoint interface for vertical slice architecture
- Auto-discovery of endpoints from assemblies
- Per-endpoint dependency injection
- Clean separation of concerns

## Documentation

Each package includes:
- Comprehensive XML documentation for IntelliSense
- README.md with usage examples
- Full API documentation
- Best practices and patterns

## Requirements

- .NET 8.0 or later (most packages)
- .NET 9.0 or later (Zircon.AspNetCore.Endpoints)

## License

All packages are licensed under the MIT License.

## Contributing

Contributions are welcome! Please see the main repository at https://github.com/adampaquette/Zircon

## Support

For issues, feature requests, or questions, please visit:
https://github.com/adampaquette/Zircon/issues

## Version History

### Version 1.0.0 (Current)
- Initial release of all packages
- Full documentation and examples
- Production-ready implementations

## Publishing Packages

```powershell
# Set your API key once
$env:NUGET_API_KEY = "your-key-here"

# Publish a package
.\publish.ps1 -Package Zircon.Results -Version 1.0.1
```

Get your API key from: https://www.nuget.org/account/apikeys

---

**Author**: Adam Paquette  
**Company**: AppStackOne  
**Repository**: https://github.com/adampaquette/Zircon