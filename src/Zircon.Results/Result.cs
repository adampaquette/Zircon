namespace Zircon.Results;

public class Result
{
    public bool IsSuccess { get; }
    public IEnumerable<string> Errors { get; }
    public Exception? Exception { get; }

    protected Result(bool isSuccess, IEnumerable<string>? errors = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<string>();
        Exception = exception;
    }

    public static Result Success() => new(true);

    public static Result Failure(Exception exception)
    {
        var errors = ExtractErrors(exception);
        return new Result(false, errors, exception);
    }

    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    protected static IEnumerable<string> ExtractErrors(Exception exception)
    {
        return ErrorExtractorRegistry.Instance.ExtractErrors(exception);
    }

    public string GetFormattedErrors(string fallback, string separator = "\n")
    {
        return Errors.Any()
            ? string.Join(separator, Errors)
            : fallback;
    }
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, IEnumerable<string>? errors = null, Exception? exception = null)
        : base(isSuccess, errors, exception)
    {
        Value = value;
    }

    public static Result<T> Success(T? value) => new(true, value);

    public new static Result<T> Failure(Exception exception)
    {
        var errors = ExtractErrors(exception);
        return new Result<T>(false, default, errors, exception);
    }

    public new static Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);
}
