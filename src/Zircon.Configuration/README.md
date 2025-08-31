# Zircon.Configuration

Configuration extension methods providing enhanced functionality for Microsoft.Extensions.Configuration including required value retrieval with validation.

## Features

- **Required Value Retrieval**: Get configuration values with automatic validation
- **Clear Error Messages**: Descriptive error messages showing the full configuration path
- **Section Support**: Works with both root configuration and configuration sections
- **Exception Safety**: Throws meaningful exceptions for missing configuration

## Installation

```bash
dotnet add package Zircon.Configuration
```

## Usage

### Basic Configuration Access

```csharp
using Microsoft.Extensions.Configuration;
using Zircon.Configuration;

// Configuration setup
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Get required values - throws if missing
string connectionString = configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");
string apiKey = configuration.GetRequiredValue("ExternalApi:ApiKey");
```

### Working with Configuration Sections

```csharp
// Access nested configuration sections
var databaseSection = configuration.GetSection("Database");
string host = databaseSection.GetRequiredValue("Host");
string port = databaseSection.GetRequiredValue("Port");

// The error messages will show the full path: "Database:Host" or "Database:Port"
```

### Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = Configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    var redisConnection = Configuration.GetRequiredValue("Redis:ConnectionString");
    services.AddStackExchangeRedisCache(options =>
        options.Configuration = redisConnection);
}
```

### Configuration Validation

```csharp
public class AppSettings
{
    public string DatabaseConnectionString { get; set; }
    public string JwtSecret { get; set; }
    public string ExternalApiUrl { get; set; }
}

public void ValidateConfiguration(IConfiguration configuration)
{
    // These will all throw with clear error messages if missing
    var settings = new AppSettings
    {
        DatabaseConnectionString = configuration.GetRequiredValue("ConnectionStrings:Database"),
        JwtSecret = configuration.GetRequiredValue("Jwt:Secret"),
        ExternalApiUrl = configuration.GetRequiredValue("ExternalServices:ApiUrl")
    };
}
```

## Error Handling

When a required configuration value is missing, `GetRequiredValue` throws an `InvalidOperationException` with a descriptive message:

```csharp
// If "Database:ConnectionString" is missing:
// InvalidOperationException: "Configuration missing value for: Database:ConnectionString"

// If "ApiKey" is missing from root configuration:
// InvalidOperationException: "Configuration missing value for: ApiKey"
```

## Extension Method Details

### GetRequiredValue

```csharp
public static string GetRequiredValue(this IConfiguration configuration, string name)
```

**Parameters:**
- `configuration`: The `IConfiguration` instance to retrieve the value from
- `name`: The configuration key name

**Returns:**
- `string`: The configuration value

**Exceptions:**
- `InvalidOperationException`: Thrown when the configuration value is null or missing

**Key Features:**
- Automatically determines if the configuration is a section and includes the full path in error messages
- Works with both `IConfiguration` and `IConfigurationSection` instances
- Provides clear, actionable error messages for missing configuration

## Comparison with Built-in Methods

| Method | Missing Value Behavior | Error Message Quality |
|--------|----------------------|----------------------|
| `configuration["key"]` | Returns `null` | No error - silent failure |
| `configuration.GetValue<string>("key")` | Returns `null` | No error - silent failure |
| `configuration.GetRequiredValue("key")` | Throws exception | Clear error with full path |

## Best Practices

1. **Use Early in Application Startup**: Call `GetRequiredValue` during application configuration to fail fast
2. **Validate Critical Settings**: Use for database connections, API keys, and other essential configuration
3. **Combine with Options Pattern**: Use alongside `IOptions<T>` for structured configuration
4. **Environment-Specific Validation**: Ensure all required configuration is present across environments

## Common Usage Patterns

### Database Configuration

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = Configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");
    options.UseSqlServer(connectionString);
});
```

### External Service Configuration

```csharp
services.AddHttpClient("ExternalApi", client =>
{
    var baseUrl = Configuration.GetRequiredValue("ExternalServices:BaseUrl");
    var apiKey = Configuration.GetRequiredValue("ExternalServices:ApiKey");
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
});
```

### JWT Configuration

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = Configuration.GetRequiredValue("Jwt:SecretKey");
        var issuer = Configuration.GetRequiredValue("Jwt:Issuer");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidIssuer = issuer,
            // ... other parameters
        };
    });
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.