# Zircon NuGet Packages

[![CI Build](https://github.com/adampaquette/Zircon/actions/workflows/ci-build.yml/badge.svg)](https://github.com/adampaquette/Zircon/actions/workflows/ci-build.yml)
[![NuGet](https://img.shields.io/nuget/v/Zircon.Results.svg)](https://www.nuget.org/packages?q=Zircon)

## Overview

The Zircon library collection provides a comprehensive set of utilities and patterns for building robust .NET applications. All packages follow consistent standards for documentation, testing, and maintainability.

## Available Packages

### ASP.NET Core

| Package | NuGet | Description |
|---------|-------|-------------|
| **Zircon.AspNetCore.Endpoints** | [![NuGet](https://img.shields.io/nuget/v/Zircon.AspNetCore.Endpoints.svg)](https://www.nuget.org/packages/Zircon.AspNetCore.Endpoints) | Vertical slice architecture for ASP.NET Core Minimal APIs |

### Core Packages

| Package | NuGet | Description |
|---------|-------|-------------|
| **Zircon.Results** | [![NuGet](https://img.shields.io/nuget/v/Zircon.Results.svg)](https://www.nuget.org/packages/Zircon.Results) | Result pattern implementation for clean error handling |
| **Zircon.Abstractions** | [![NuGet](https://img.shields.io/nuget/v/Zircon.Abstractions.svg)](https://www.nuget.org/packages/Zircon.Abstractions) | Core DDD abstractions including auditable entities and domain events |
| **Zircon.Configuration** | [![NuGet](https://img.shields.io/nuget/v/Zircon.Configuration.svg)](https://www.nuget.org/packages/Zircon.Configuration) | Enhanced configuration extensions with required value validation |
| **Zircon.Security.Claims.Extensions** | [![NuGet](https://img.shields.io/nuget/v/Zircon.Security.Claims.Extensions.svg)](https://www.nuget.org/packages/Zircon.Security.Claims.Extensions) | ClaimsPrincipal authentication utilities |

### Infrastructure

| Package | NuGet | Description |
|---------|-------|-------------|
| **Zircon.MediatR** | [![NuGet](https://img.shields.io/nuget/v/Zircon.MediatR.svg)](https://www.nuget.org/packages/Zircon.MediatR) | MediatR pipeline behaviors with performance monitoring |
| **Zircon.Serialization** | [![NuGet](https://img.shields.io/nuget/v/Zircon.Serialization.svg)](https://www.nuget.org/packages/Zircon.Serialization) | JSON converters for System.Text.Json |
| **Zircon.IO** | [![NuGet](https://img.shields.io/nuget/v/Zircon.IO.svg)](https://www.nuget.org/packages/Zircon.IO) | Stream extension methods for async I/O operations |
| **Zircon.Diagnostics** | [![NuGet](https://img.shields.io/nuget/v/Zircon.Diagnostics.svg)](https://www.nuget.org/packages/Zircon.Diagnostics) | OpenTelemetry activity extensions for exception tracking |

### Development & Testing

| Package | NuGet | Description |
|---------|-------|-------------|
| **Zircon.Test** | [![NuGet](https://img.shields.io/nuget/v/Zircon.Test.svg)](https://www.nuget.org/packages/Zircon.Test) | Testing utilities with AutoFixture and AutoMoq integration |

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

## Requirements

- .NET 10.0 or later

## License

All packages are licensed under the MIT License.

## Contributing

Contributions are welcome! Please see the main repository at https://github.com/adampaquette/Zircon

## Support

For issues, feature requests, or questions, please visit:
https://github.com/adampaquette/Zircon/issues

## Version History

### Version 1.1.0
- Upgrade to .NET 10
- Updated all dependencies to latest versions
- Centralized versioning with automated NuGet publishing

### Version 1.0.0
- Initial release of all packages

---

**Author**: Adam Paquette
**Company**: AppStackOne
**Repository**: https://github.com/adampaquette/Zircon
