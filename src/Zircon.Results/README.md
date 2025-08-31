# Zircon.Results

A robust result pattern implementation for C# applications providing a clean way to handle success and failure outcomes with comprehensive error aggregation and exception handling.

## Features

- **Result Pattern Implementation**: Clean separation of success and failure states
- **Error Aggregation**: Collect and manage multiple error messages
- **Exception Handling**: Automatic error extraction from exceptions
- **Generic Support**: Strongly typed results with `Result<T>` for operations returning values
- **Extensible Error Extraction**: Pluggable system for custom exception error extraction

## Installation

```bash
dotnet add package Zircon.Results
```

## Basic Usage

### Simple Operations

```csharp
using Zircon.Results;

// Success case
var successResult = Result.Success();
if (successResult.IsSuccess)
{
    Console.WriteLine("Operation completed successfully!");
}

// Failure with custom errors
var failureResult = Result.Failure(new[] { "Invalid input", "Missing required field" });
if (!failureResult.IsSuccess)
{
    Console.WriteLine($"Errors: {failureResult.GetFormattedErrors("No errors")}");
}
```

### Operations with Return Values

```csharp
// Success with value
var successWithValue = Result<string>.Success("Hello, World!");
if (successWithValue.IsSuccess)
{
    Console.WriteLine($"Result: {successWithValue.Value}");
}

// Failure for generic result
var failureWithValue = Result<int>.Failure(new[] { "Could not parse number" });
```

### Exception Handling

```csharp
try
{
    // Some operation that might throw
    var data = ProcessData();
    return Result<string>.Success(data);
}
catch (Exception ex)
{
    return Result<string>.Failure(ex);
}
```

## Key Classes

### Result
Base class for operation results without return values.

**Properties:**
- `IsSuccess`: Indicates if the operation was successful
- `Errors`: Collection of error messages
- `Exception`: The original exception that caused failure (if any)

**Methods:**
- `Success()`: Creates a successful result
- `Failure(Exception)`: Creates a failed result from an exception  
- `Failure(IEnumerable<string>)`: Creates a failed result with error messages
- `GetFormattedErrors(fallback, separator)`: Gets formatted error string

### Result\<T\>
Generic result class for operations that return a value on success.

**Additional Properties:**
- `Value`: The returned value (only valid when `IsSuccess` is true)

**Additional Methods:**
- `Success(T value)`: Creates a successful result with a value
- `Failure(Exception)`: Creates a failed result from an exception
- `Failure(IEnumerable<string>)`: Creates a failed result with error messages

## Custom Error Extraction

Implement `IExceptionErrorExtractor` to provide custom error extraction logic for specific exception types:

```csharp
public class ValidationExceptionExtractor : IExceptionErrorExtractor
{
    public bool CanExtract(Exception exception) => exception is ValidationException;
    
    public IEnumerable<string> ExtractErrors(Exception exception)
    {
        if (exception is ValidationException validationEx)
        {
            return validationEx.Errors.Select(e => e.ErrorMessage);
        }
        return new[] { exception.Message };
    }
}
```

## Best Practices

1. **Prefer Result Types**: Use Result/Result<T> instead of throwing exceptions for expected failure scenarios
2. **Check Success**: Always check `IsSuccess` before accessing `Value` in Result<T>
3. **Handle Errors Gracefully**: Use `GetFormattedErrors()` to present user-friendly error messages
4. **Avoid Exceptions for Flow Control**: Use Result pattern for business logic failures, reserve exceptions for unexpected errors

## License

This project is licensed under the MIT License - see the LICENSE file for details.