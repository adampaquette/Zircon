# Zircon.Serialization

JSON serialization utilities and converters for System.Text.Json including custom decimal converters with culture-aware parsing and robust error handling.

## Features

- **Decimal Converter**: Custom nullable decimal converter with culture-aware parsing
- **Robust Parsing**: Handles both numeric and string representations of decimals
- **Culture-Independent**: Converts comma decimal separators to dots automatically  
- **Null Safety**: Proper handling of null and empty values
- **System.Text.Json Integration**: Seamless integration with .NET's built-in JSON serializer

## Installation

```bash
dotnet add package Zircon.Serialization
```

## Usage

### Basic Decimal Conversion

```csharp
using System.Text.Json;
using Zircon.Serialization;

// Configure JsonSerializerOptions with the decimal converter
var options = new JsonSerializerOptions
{
    Converters = { new DecimalConverter() }
};

// Serialize object with decimal values
var product = new Product { Name = "Laptop", Price = 999.99m };
string json = JsonSerializer.Serialize(product, options);

// Deserialize JSON with decimal values
var deserializedProduct = JsonSerializer.Deserialize<Product>(json, options);
```

### Web API Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<JsonOptions>(options =>
    {
        options.SerializerOptions.Converters.Add(new DecimalConverter());
    });
    
    // For Minimal APIs
    services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new DecimalConverter());
    });
}
```

### ASP.NET Core Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        // The DecimalConverter will handle decimal parsing automatically
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price, // Handles "1,234.56", "1234.56", 1234.56
            Cost = request.Cost
        };
        
        await _productService.CreateAsync(product);
        return Ok(product);
    }
}

public class CreateProductRequest
{
    public string Name { get; set; }
    public decimal? Price { get; set; }  // Nullable decimal - converter handles nulls
    public decimal Cost { get; set; }    // Non-nullable - use separate converter if needed
}
```

## Supported Input Formats

The `DecimalConverter` can parse various decimal formats:

### JSON Number Values
```json
{
  "price": 123.45,
  "cost": 1000,
  "discount": null
}
```

### JSON String Values with Different Formats
```json
{
  "price": "123.45",      // Standard decimal
  "cost": "1,234.56",     // Comma separator (converted to dot)
  "discount": "1234",     // Integer as string
  "tax": "",              // Empty string (becomes null)
  "shipping": null        // Explicit null
}
```

### Localized Number Formats
```csharp
// These JSON strings will all be parsed correctly:
var testCases = new[]
{
    """{"value": "123.45"}""",      // US format
    """{"value": "123,45"}""",      // European format (comma converted to dot)
    """{"value": "1.234,56"}""",    // German format  
    """{"value": "1,234.56"}""",    // US thousands separator
    """{"value": null}""",          // Null value
    """{"value": ""}"""             // Empty string -> null
};

foreach (var json in testCases)
{
    var result = JsonSerializer.Deserialize<TestObject>(json, options);
    // All will deserialize correctly
}
```

## Advanced Usage

### Custom Configuration

```csharp
public class CustomJsonSerializerService
{
    private readonly JsonSerializerOptions _options;
    
    public CustomJsonSerializerService()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = 
            {
                new DecimalConverter(),
                // Add other custom converters here
            }
        };
    }
    
    public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
    public string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, _options);
}
```

### Financial Data Processing

```csharp
public class FinancialDataProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new DecimalConverter() }
    };
    
    public async Task<List<Transaction>> ProcessTransactionFile(Stream jsonStream)
    {
        var transactions = await JsonSerializer.DeserializeAsync<List<Transaction>>(
            jsonStream, 
            JsonOptions);
            
        return transactions.Where(t => t.Amount.HasValue && t.Amount > 0).ToList();
    }
}

public class Transaction
{
    public string Id { get; set; }
    public decimal? Amount { get; set; }    // Handled by DecimalConverter
    public decimal? Fee { get; set; }       // Handled by DecimalConverter
    public DateTime Date { get; set; }
}
```

### API Client Integration

```csharp
public class PricingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public PricingApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new DecimalConverter() }
        };
    }
    
    public async Task<PriceQuote> GetPriceQuoteAsync(string productId)
    {
        var response = await _httpClient.GetAsync($"/api/pricing/{productId}");
        var json = await response.Content.ReadAsStringAsync();
        
        // DecimalConverter will handle various decimal formats from external API
        return JsonSerializer.Deserialize<PriceQuote>(json, _jsonOptions);
    }
}

public class PriceQuote
{
    public decimal? BasePrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public decimal? Tax { get; set; }
}
```

## Error Handling

The converter provides detailed error information when parsing fails:

```csharp
try
{
    var json = """{"price": "invalid-number"}""";
    var result = JsonSerializer.Deserialize<Product>(json, options);
}
catch (JsonException ex)
{
    // Error message: "Impossible de convertir en decimal. Token: String"
    Console.WriteLine($"JSON parsing failed: {ex.Message}");
}
```

### Robust Error Handling Pattern

```csharp
public class SafeJsonDeserializer
{
    public static Result<T> TryDeserialize<T>(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new DecimalConverter() }
            };
            
            var result = JsonSerializer.Deserialize<T>(json, options);
            return Result<T>.Success(result);
        }
        catch (JsonException ex)
        {
            return Result<T>.Failure(new[] { $"JSON deserialization failed: {ex.Message}" });
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }
}

// Usage
var result = SafeJsonDeserializer.TryDeserialize<Product>(jsonString);
if (result.IsSuccess)
{
    var product = result.Value;
    // Process product
}
else
{
    // Handle errors
    Console.WriteLine(result.GetFormattedErrors("Unknown error"));
}
```

## Converter Details

### DecimalConverter Class

```csharp
public class DecimalConverter : JsonConverter<decimal?>
```

**Key Features:**
- **Null Handling**: Properly processes JSON null tokens
- **Number Tokens**: Direct conversion of JSON number values
- **String Tokens**: Parsing of string representations with validation
- **Culture Conversion**: Automatic comma-to-dot conversion for decimal separators
- **Empty String Handling**: Treats empty/whitespace strings as null
- **Error Reporting**: Clear error messages in French (easily localizable)

**Supported Input Types:**
- `JsonTokenType.Null` → `null`
- `JsonTokenType.Number` → Direct decimal conversion
- `JsonTokenType.String` → Parsed with culture-aware logic

## Integration Patterns

### Dependency Injection

```csharp
public interface IJsonSerializationService
{
    T Deserialize<T>(string json);
    string Serialize<T>(T obj);
}

public class JsonSerializationService : IJsonSerializationService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new DecimalConverter() }
    };
    
    public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
    public string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, Options);
}

// Registration
services.AddSingleton<IJsonSerializationService, JsonSerializationService>();
```

### Configuration Pattern

```csharp
public class JsonConfiguration
{
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters = 
            {
                new DecimalConverter(),
                new JsonStringEnumConverter()
            }
        };
    }
}

// Usage throughout application
var options = JsonConfiguration.CreateDefaultOptions();
```

## Performance Considerations

- **Memory Efficient**: Minimal object allocation during conversion
- **Culture-Independent**: Uses `CultureInfo.InvariantCulture` for consistent parsing
- **String Processing**: Efficient string manipulation for comma replacement
- **Exception Handling**: Only throws when truly unable to convert (fail-fast approach)

## Best Practices

1. **Global Registration**: Register the converter globally in ASP.NET Core applications
2. **Consistent Usage**: Use the same JsonSerializerOptions throughout your application
3. **Error Handling**: Wrap JSON operations in try-catch blocks for production code
4. **Testing**: Test with various decimal formats to ensure compatibility
5. **Null Safety**: Use nullable decimals (`decimal?`) when values might be null

## Common Use Cases

- **Financial Applications**: Handle monetary values from various sources
- **International APIs**: Process decimal data from different locales
- **Configuration Files**: Parse configuration with decimal values safely
- **Data Import/Export**: Handle CSV-to-JSON conversions with decimal values
- **Multi-tenant Applications**: Support different number formats per tenant

## License

This project is licensed under the MIT License - see the LICENSE file for details.