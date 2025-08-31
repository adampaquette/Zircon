# Zircon.MediatR

MediatR extensions and behaviors including performance metric collection, validation pipeline behaviors, and dependency injection utilities for enhanced CQRS implementations.

## Features

- **Performance Metrics**: Automatic performance tracking for MediatR handlers
- **Pipeline Behaviors**: Ready-to-use behaviors for cross-cutting concerns
- **Dependency Injection**: Simple service registration extensions
- **Observability**: Integration with logging and metrics collection
- **CQRS Support**: Enhanced support for Command Query Responsibility Segregation patterns

## Installation

```bash
dotnet add package Zircon.MediatR
```

## Quick Start

### Service Registration

```csharp
using Zircon.MediatR.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    // Register MediatR
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    
    // Add Zircon MediatR extensions
    services.AddZirconMediatR();
}
```

### Basic Usage

```csharp
public class CreateUserCommand : IRequest<Result<User>>
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<User>>
{
    // Performance metrics will be automatically collected
    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Handler logic here
        var user = new User { Name = request.Name, Email = request.Email };
        await _userRepository.SaveAsync(user);
        
        return Result<User>.Success(user);
    }
}
```

## Features

### Performance Metric Collection

The `HandlerPerformanceMetricBehaviour` automatically tracks execution time and performance data for all MediatR handlers:

```csharp
// Automatically applied to all handlers
public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}

public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Execution time will be automatically measured and logged
        return await _userRepository.GetByIdAsync(request.UserId);
    }
}
```

### Custom Performance Handling

```csharp
public class CustomPerformanceService
{
    private readonly PerformanceMetricHandler _performanceHandler;
    
    public CustomPerformanceService(PerformanceMetricHandler performanceHandler)
    {
        _performanceHandler = performanceHandler;
    }
    
    public async Task ProcessWithMetrics()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Your business logic
            await DoSomeWork();
        }
        finally
        {
            stopwatch.Stop();
            await _performanceHandler.HandlePerformanceMetricAsync(
                "CustomOperation", 
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## Pipeline Behaviors

### Performance Behavior

Automatically measures and logs execution time for all handlers:

```csharp
// This behavior is automatically registered
public class SomeQuery : IRequest<SomeResult>
{
    // Query properties
}

// Execution metrics will be captured automatically
[Log Output]
// Information: Executing handler for SomeQuery
// Information: Handler for SomeQuery completed in 125ms
```

### Adding Custom Behaviors

```csharp
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}

// Register additional behaviors
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
```

## Advanced Usage

### Query with Performance Tracking

```csharp
public class GetOrdersByCustomerQuery : IRequest<List<Order>>
{
    public int CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetOrdersByCustomerHandler : IRequestHandler<GetOrdersByCustomerQuery, List<Order>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrdersByCustomerHandler> _logger;

    public GetOrdersByCustomerHandler(
        IOrderRepository orderRepository, 
        ILogger<GetOrdersByCustomerHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<List<Order>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving orders for customer {CustomerId}", request.CustomerId);
        
        var orders = await _orderRepository.GetOrdersByCustomerAsync(
            request.CustomerId, 
            request.FromDate, 
            request.ToDate, 
            cancellationToken);
            
        _logger.LogInformation("Found {OrderCount} orders for customer {CustomerId}", 
            orders.Count, request.CustomerId);
            
        return orders;
    }
}
```

### Command with Result Pattern

```csharp
using Zircon.Results;

public class UpdateProductCommand : IRequest<Result>
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return Result.Failure(new[] { $"Product {request.ProductId} not found" });
            }

            product.UpdateDetails(request.Name, request.Price);
            await _productRepository.SaveAsync(product);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }
    }
}
```

### Notification Handlers

```csharp
public class OrderCreatedNotification : INotification
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
}

public class EmailNotificationHandler : INotificationHandler<OrderCreatedNotification>
{
    // Performance metrics are tracked for notification handlers too
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        await _emailService.SendOrderConfirmationAsync(
            notification.CustomerId, 
            notification.OrderId);
    }
}

public class AuditLogHandler : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        await _auditService.LogEventAsync(
            "OrderCreated", 
            notification.OrderId, 
            notification.CustomerId);
    }
}
```

## Service Registration Details

### AddZirconMediatR()

The `AddZirconMediatR()` extension method registers:

- `HandlerPerformanceMetricBehaviour<,>` as a scoped pipeline behavior
- `PerformanceMetricHandler` as a scoped service

```csharp
public static IServiceCollection AddZirconMediatR(this IServiceCollection services)
{
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(HandlerPerformanceMetricBehaviour<,>));
    services.AddScoped<PerformanceMetricHandler>();
    return services;
}
```

### Custom Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Standard MediatR registration
    services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        cfg.AddBehavior<ValidationBehaviour<,>>();
        cfg.AddBehavior<LoggingBehaviour<,>>();
    });
    
    // Add Zircon extensions
    services.AddZirconMediatR();
    
    // Additional custom behaviors
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehaviour<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
}
```

## Performance Monitoring

### Metrics Collection

The performance behavior automatically collects:
- Handler execution time
- Handler type information
- Request type information
- Success/failure status

### Integration with Application Insights

```csharp
public class ApplicationInsightsPerformanceHandler : PerformanceMetricHandler
{
    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightsPerformanceHandler(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public override async Task HandlePerformanceMetricAsync(string operationName, long durationMs)
    {
        _telemetryClient.TrackDependency("MediatR", operationName, 
            DateTime.UtcNow.AddMilliseconds(-durationMs), 
            TimeSpan.FromMilliseconds(durationMs), true);
            
        await base.HandlePerformanceMetricAsync(operationName, durationMs);
    }
}

// Register custom handler
services.AddScoped<PerformanceMetricHandler, ApplicationInsightsPerformanceHandler>();
```

### Custom Metrics Dashboard

```csharp
[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly PerformanceMetricHandler _metricsHandler;

    [HttpGet("handler-performance")]
    public async Task<IActionResult> GetHandlerPerformance()
    {
        var metrics = await _metricsHandler.GetPerformanceMetricsAsync();
        return Ok(metrics);
    }
}
```

## Best Practices

1. **Handler Responsibilities**: Keep handlers focused on a single responsibility
2. **Error Handling**: Use Result pattern for expected failures, exceptions for unexpected errors
3. **Performance Monitoring**: Let the behavior track performance automatically
4. **Validation**: Add validation behaviors for input validation
5. **Logging**: Use structured logging in handlers for better observability
6. **Testing**: Mock IMediator interface for testing controllers and services

## Testing

### Unit Testing Handlers

```csharp
public class CreateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _handler = new CreateUserHandler(_userRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var command = new CreateUserCommand { Name = "John Doe", Email = "john@example.com" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Once);
    }
}
```

### Integration Testing

```csharp
public class MediatRIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Fact]
    public async Task SendCommand_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var command = new CreateUserCommand { Name = "Test User", Email = "test@example.com" };

        // Act
        var result = await mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.