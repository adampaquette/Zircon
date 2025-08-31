# Zircon.AspNetCore.Endpoints

A lightweight library for organizing ASP.NET Core Minimal APIs using a vertical slice architecture pattern. Each endpoint is self-contained with its own routing, handling logic, and dependency registration.

## Features

- 🎯 **Vertical Slice Architecture** - Each endpoint is a self-contained unit
- 🔧 **Per-Endpoint Dependencies** - Register services specific to each endpoint
- 🚀 **Auto-Discovery** - Automatically find and register endpoints from assemblies
- 📦 **Minimal Overhead** - Lightweight abstraction over ASP.NET Core Minimal APIs
- 🧩 **Clean Separation** - Keep your Program.cs clean and organized

## Installation

```bash
dotnet add package Zircon.AspNetCore.Endpoints
```

## Quick Start

### 1. Create an Endpoint

```csharp
using Zircon.AspNetCore.Endpoints;

public class GetWeatherEndpoint : IEndpoint
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/weather", GetWeather)
            .WithName("GetWeather")
            .WithTags("Weather")
            .Produces<WeatherForecast[]>();
    }

    private static WeatherForecast[] GetWeather()
    {
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot" };
        return Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]))
            .ToArray();
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
```

### 2. Register and Map Endpoints in Program.cs

```csharp
using Zircon.AspNetCore.Endpoints.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register all endpoints from the current assembly
builder.Services.AddEndpointsFromAssemblyContaining<Program>();

var app = builder.Build();

// Map all registered endpoints
app.MapEndpoints();

app.Run();
```

## Advanced Usage

### Endpoint with Dependencies

Create endpoints that register and use their own dependencies:

```csharp
public class CreateProductEndpoint : IEndpoint
{
    // Register endpoint-specific services
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IProductValidator, ProductValidator>();
        services.AddScoped<IInventoryService, InventoryService>();
    }

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/api/products", CreateProduct)
            .WithName("CreateProduct")
            .WithTags("Products")
            .Produces<ProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .RequireAuthorization();
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        IProductValidator validator,
        IInventoryService inventory,
        ProductDbContext db,
        CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.Errors);
        }

        // Check inventory
        if (!await inventory.IsAvailableAsync(request.Sku, cancellationToken))
        {
            return Results.Problem("Product SKU is not available", statusCode: 409);
        }

        // Create and save product
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Sku = request.Sku,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        var response = new ProductResponse(product.Id, product.Name, product.Sku, product.Price);
        return Results.Created($"/api/products/{product.Id}", response);
    }
}

public record CreateProductRequest(string Name, string Sku, decimal Price);
public record ProductResponse(Guid Id, string Name, string Sku, decimal Price);
```

### Manual Endpoint Mapping

For testing or specific scenarios, you can map endpoints manually:

```csharp
// Map specific endpoints without DI registration
app.MapEndpoint<GetWeatherEndpoint>();
app.MapEndpoint<GetHealthEndpoint>();
```

## Benefits

- **Organized Code**: Each feature is self-contained in its own file/folder
- **Testability**: Endpoints are easy to test in isolation
- **Maintainability**: Changes to one endpoint don't affect others
- **Discoverability**: All endpoint logic is in one place
- **Dependency Isolation**: Each endpoint can have its own services
- **Clean Program.cs**: No endpoint mapping clutter in startup code

## Requirements

- .NET 8.0 or later
- ASP.NET Core 8.0 or later

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or suggestions, please create an issue in the GitHub repository.