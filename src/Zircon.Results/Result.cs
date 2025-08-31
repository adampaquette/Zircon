namespace Zircon.Results;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// Provides error aggregation and exception handling capabilities.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// Gets the collection of errors that occurred during the operation.
    /// </summary>
    public IEnumerable<string> Errors { get; }
    
    /// <summary>
    /// Gets the exception that caused the operation to fail, if any.
    /// </summary>
    public Exception? Exception { get; }

    protected Result(bool isSuccess, IEnumerable<string>? errors = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<string>();
        Exception = exception;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed <see cref="Result"/> containing extracted error messages.</returns>
    public static Result Failure(Exception exception)
    {
        var errors = ExtractErrors(exception);
        return new Result(false, errors, exception);
    }

    /// <summary>
    /// Creates a failed result with the specified error messages.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed <see cref="Result"/> containing the specified errors.</returns>
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    protected static IEnumerable<string> ExtractErrors(Exception exception)
    {
        return ErrorExtractorRegistry.Instance.ExtractErrors(exception);
    }

    /// <summary>
    /// Gets a formatted string of all errors, or a fallback string if no errors exist.
    /// </summary>
    /// <param name="fallback">The fallback string to return if no errors exist.</param>
    /// <param name="separator">The separator to use between error messages. Defaults to newline.</param>
    /// <returns>A formatted string of errors or the fallback string.</returns>
    public string GetFormattedErrors(string fallback, string separator = "\n")
    {
        return Errors.Any()
            ? string.Join(separator, Errors)
            : fallback;
    }
}

/// <summary>
/// Represents the result of an operation that can either succeed or fail and returns a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value returned by the operation, if successful.
    /// </summary>
    public T? Value { get; }

    private Result(bool isSuccess, T? value, IEnumerable<string>? errors = null, Exception? exception = null)
        : base(isSuccess, errors, exception)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing the specified value.</returns>
    public static Result<T> Success(T? value) => new(true, value);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> containing extracted error messages.</returns>
    public new static Result<T> Failure(Exception exception)
    {
        var errors = ExtractErrors(exception);
        return new Result<T>(false, default, errors, exception);
    }

    /// <summary>
    /// Creates a failed result with the specified error messages.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed <see cref="Result{T}"/> containing the specified errors.</returns>
    public new static Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);
}
