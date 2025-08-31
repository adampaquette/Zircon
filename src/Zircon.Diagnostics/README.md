# Zircon.Diagnostics

Diagnostic extensions for OpenTelemetry activities including exception tagging and status management following OpenTelemetry semantic conventions.

## Features

- **OpenTelemetry Compliance**: Follows OpenTelemetry semantic conventions for exception handling
- **Activity Enhancement**: Extends System.Diagnostics.Activity with exception-specific functionality
- **Null Safety**: Safe handling of null Activity instances
- **Comprehensive Exception Data**: Captures exception message, stack trace, and type information
- **Status Management**: Automatically sets activity status to Error when exceptions occur

## Installation

```bash
dotnet add package Zircon.Diagnostics
```

## Usage

### Basic Exception Handling

```csharp
using System.Diagnostics;
using Zircon.Diagnostics;

public class OrderService
{
    private static readonly ActivitySource ActivitySource = new("OrderService");
    
    public async Task ProcessOrderAsync(Order order)
    {
        using var activity = ActivitySource.StartActivity("ProcessOrder");
        
        try
        {
            // Process the order
            await ValidateOrder(order);
            await SaveOrder(order);
            await SendConfirmation(order);
        }
        catch (Exception ex)
        {
            // This will add exception tags and set status to Error
            activity.SetExceptionTags(ex);
            throw; // Re-throw to maintain exception flow
        }
    }
}
```

### Web API Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("ProductsApi");
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        using var activity = ActivitySource.StartActivity("CreateProduct");
        activity?.SetTag("product.name", request.Name);
        
        try
        {
            var product = await _productService.CreateProductAsync(request);
            activity?.SetTag("product.id", product.Id.ToString());
            
            return Created($"/api/products/{product.Id}", product);
        }
        catch (ValidationException ex)
        {
            activity.SetExceptionTags(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            activity.SetExceptionTags(ex);
            return StatusCode(500, "An error occurred while creating the product");
        }
    }
}
```

### Middleware Integration

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Get current activity and add exception information
            Activity.Current.SetExceptionTags(ex);
            
            _logger.LogError(ex, "An unhandled exception occurred");
            
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal Server Error");
        }
    }
}
```

### Background Service Usage

```csharp
public class DataProcessingService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("DataProcessing");
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = ActivitySource.StartActivity("ProcessBatch");
            
            try
            {
                await ProcessNextBatch();
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity.SetExceptionTags(ex);
                // Log and continue processing
                _logger.LogError(ex, "Error processing batch");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Extension Methods

### SetExceptionTags

```csharp
public static void SetExceptionTags(this Activity? activity, Exception ex)
```

**Description:** Sets exception-related tags on an activity following OpenTelemetry semantic conventions.

**Parameters:**
- `activity`: The activity to set tags on (null-safe)
- `ex`: The exception to extract information from

**Tags Added:**
- `exception.message`: The exception message
- `exception.stacktrace`: The full exception string (including stack trace)
- `exception.type`: The full type name of the exception

**Additional Actions:**
- Sets the activity status to `ActivityStatusCode.Error`

## OpenTelemetry Semantic Conventions

This package follows the [OpenTelemetry Semantic Conventions for Exceptions](https://opentelemetry.io/docs/specs/otel/trace/semantic_conventions/exceptions/):

| Tag | Description | Example |
|-----|-------------|---------|
| `exception.message` | The exception message | "Validation failed for field 'Email'" |
| `exception.stacktrace` | Full exception details | Full stack trace string |
| `exception.type` | Exception type name | "System.ArgumentNullException" |

## Configuration with OpenTelemetry

### ASP.NET Core Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("MyService"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("OrderService") // Add your custom activity sources
            .AddSource("ProductsApi")
            .AddJaegerExporter());
}
```

### Console Application Setup

```csharp
using var tracerProvider = TracerProviderBuilder.Create()
    .AddSource("DataProcessing")
    .SetSampler(new AlwaysOnSampler())
    .AddConsoleExporter()
    .Build();

// Your application code here
```

## Advanced Usage

### Custom Exception Handling

```csharp
public class BusinessLogicService
{
    private static readonly ActivitySource ActivitySource = new("BusinessLogic");
    
    public async Task ProcessBusinessRuleAsync(BusinessRequest request)
    {
        using var activity = ActivitySource.StartActivity("ProcessBusinessRule");
        activity?.SetTag("rule.type", request.RuleType);
        
        try
        {
            await ValidateBusinessRules(request);
            await ExecuteBusinessLogic(request);
        }
        catch (BusinessRuleException ex)
        {
            activity.SetExceptionTags(ex);
            activity?.SetTag("business.rule.violation", ex.RuleName);
            throw;
        }
        catch (Exception ex)
        {
            activity.SetExceptionTags(ex);
            activity?.SetTag("error.category", "system");
            throw;
        }
    }
}
```

### Conditional Exception Tagging

```csharp
public class PaymentService
{
    public async Task ProcessPaymentAsync(PaymentRequest request)
    {
        using var activity = ActivitySource.StartActivity("ProcessPayment");
        
        try
        {
            await ChargeCard(request);
        }
        catch (PaymentDeclinedException ex)
        {
            activity.SetExceptionTags(ex);
            activity?.SetTag("payment.declined.reason", ex.DeclineReason);
            // Don't re-throw - this is expected behavior
            
            return new PaymentResult { Success = false, Reason = ex.DeclineReason };
        }
        catch (Exception ex)
        {
            activity.SetExceptionTags(ex);
            throw; // Unexpected errors should be thrown
        }
    }
}
```

## Integration with Logging

```csharp
public class ServiceWithLogging
{
    private readonly ILogger<ServiceWithLogging> _logger;
    
    public async Task ProcessAsync()
    {
        using var activity = ActivitySource.StartActivity("Process");
        var correlationId = Guid.NewGuid().ToString();
        activity?.SetTag("correlation.id", correlationId);
        
        try
        {
            await DoWork();
        }
        catch (Exception ex)
        {
            activity.SetExceptionTags(ex);
            
            // Log with correlation ID from activity
            _logger.LogError(ex, "Processing failed for correlation {CorrelationId}", correlationId);
            
            throw;
        }
    }
}
```

## Best Practices

1. **Always Re-throw**: Use `SetExceptionTags` for observability but still re-throw exceptions to maintain proper error flow
2. **Add Context Tags**: Include relevant business context tags before handling exceptions
3. **Use in Finally Blocks**: Consider using in finally blocks for cleanup operations
4. **Null Safety**: The extension method handles null activities gracefully
5. **Consistent Activity Sources**: Use consistent activity source names across your application
6. **Structured Logging**: Combine with structured logging for comprehensive observability

## Performance Considerations

- The extension method performs minimal work and has negligible performance impact
- Activity tag operations are optimized by the underlying OpenTelemetry libraries
- Consider sampling strategies for high-throughput applications
- Exception tags are only set when exceptions actually occur, so normal operation has no overhead

## Monitoring and Alerting

With proper OpenTelemetry configuration, you can:

- Set up alerts on high exception rates
- Create dashboards showing exception types and frequencies
- Trace request flows that include exceptions
- Correlate exceptions across multiple services
- Monitor error rates by service, endpoint, or business operation

## License

This project is licensed under the MIT License - see the LICENSE file for details.