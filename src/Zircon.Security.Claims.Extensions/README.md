# Zircon.Security.Claims.Extensions

Extension methods for ClaimsPrincipal providing convenient authentication and authorization utilities for ASP.NET Core applications.

## Features

- **Authentication Checks**: Simplified authentication status verification
- **Null Safety**: Safe handling of null ClaimsPrincipal instances
- **Clean API**: Extension methods that improve code readability
- **ASP.NET Core Integration**: Designed for seamless integration with ASP.NET Core applications

## Installation

```bash
dotnet add package Zircon.Security.Claims.Extensions
```

## Usage

### Basic Authentication Checks

```csharp
using System.Security.Claims;
using Zircon.Security.Claims.Extensions;

public class HomeController : Controller
{
    public IActionResult Dashboard()
    {
        if (User.IsAuthenticated())
        {
            return View("Dashboard");
        }
        
        return RedirectToAction("Login", "Account");
    }
}
```

### Middleware Usage

```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    
    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.IsAuthenticated())
        {
            // User is authenticated, continue processing
            await _next(context);
        }
        else
        {
            // Redirect to login or return 401
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authentication required");
        }
    }
}
```

### Razor Views

```html
@using Zircon.Security.Claims.Extensions

@if (User.IsAuthenticated())
{
    <div class="user-menu">
        <p>Welcome, authenticated user!</p>
        <a href="/profile">Profile</a>
        <a href="/logout">Logout</a>
    </div>
}
else
{
    <div class="login-prompt">
        <a href="/login">Login</a>
        <a href="/register">Register</a>
    </div>
}
```

### API Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        if (!User.IsAuthenticated())
        {
            return Unauthorized("Authentication required");
        }
        
        // Return user profile data
        return Ok(new { UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value });
    }
}
```

### Custom Authorization Logic

```csharp
public class SecurityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public SecurityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public bool CanAccessResource()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (!user.IsAuthenticated())
        {
            return false;
        }
        
        // Additional authorization logic here
        return user.HasClaim("permission", "read");
    }
}
```

## Extension Methods

### IsAuthenticated()

```csharp
public static bool IsAuthenticated(this ClaimsPrincipal principal)
```

**Description:** Checks if the current principal is authenticated.

**Parameters:**
- `principal`: The ClaimsPrincipal to check

**Returns:**
- `bool`: `true` if the principal is authenticated; otherwise, `false`

**Key Features:**
- **Null Safety**: Returns `false` if the principal or its Identity is null
- **Clean Syntax**: More readable than `principal.Identity?.IsAuthenticated ?? false`
- **Consistent Behavior**: Handles edge cases gracefully

## Advantages Over Built-in Methods

| Approach | Code | Null Safety | Readability |
|----------|------|-------------|-------------|
| Built-in | `User.Identity?.IsAuthenticated ?? false` | Manual | Verbose |
| Extension | `User.IsAuthenticated()` | Automatic | Clean |

## Integration with ASP.NET Core Features

### Authorization Attributes

```csharp
[ApiController]
public class SecureController : ControllerBase
{
    [HttpGet]
    [Authorize] // Still use [Authorize] for declarative security
    public IActionResult SecureAction()
    {
        // Additional runtime checks if needed
        if (!User.IsAuthenticated())
        {
            // This shouldn't happen with [Authorize], but defensive programming
            return Unauthorized();
        }
        
        return Ok("Secure data");
    }
}
```

### Custom Authorization Requirements

```csharp
public class CustomAuthorizationHandler : AuthorizationHandler<CustomRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomRequirement requirement)
    {
        if (context.User.IsAuthenticated() && 
            context.User.HasClaim("department", "IT"))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

### Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // JWT configuration
    });
    
    // The extension methods work with any authentication scheme
    services.AddHttpContextAccessor(); // If you need access outside controllers
}
```

## Best Practices

1. **Combine with [Authorize]**: Use extension methods for runtime checks in addition to declarative authorization
2. **Null Safety**: The extension methods handle null principals gracefully
3. **Performance**: The extension methods are lightweight and don't add significant overhead
4. **Consistency**: Use extension methods consistently throughout your application for better maintainability

## Common Patterns

### Conditional Content Rendering

```csharp
public class NavigationService
{
    public List<MenuItem> GetMenuItems(ClaimsPrincipal user)
    {
        var items = new List<MenuItem>
        {
            new MenuItem("Home", "/"),
            new MenuItem("About", "/about")
        };
        
        if (user.IsAuthenticated())
        {
            items.Add(new MenuItem("Dashboard", "/dashboard"));
            items.Add(new MenuItem("Profile", "/profile"));
        }
        else
        {
            items.Add(new MenuItem("Login", "/login"));
        }
        
        return items;
    }
}
```

### API Response Customization

```csharp
[HttpGet]
public IActionResult GetData()
{
    var data = _dataService.GetPublicData();
    
    if (User.IsAuthenticated())
    {
        // Add additional data for authenticated users
        data.PrivateData = _dataService.GetPrivateData(User.GetUserId());
    }
    
    return Ok(data);
}
```

## Future Extensions

This package provides a foundation for additional security-related extension methods. Future versions may include:

- Role-based authorization helpers
- Claim value extraction utilities  
- Permission checking extensions
- Multi-tenant authentication support

## License

This project is licensed under the MIT License - see the LICENSE file for details.