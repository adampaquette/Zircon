namespace Zircon.Results;

/// <summary>
/// Defines a contract for extracting error messages from exceptions.
/// Implementations can provide custom logic for specific exception types.
/// </summary>
public interface IExceptionErrorExtractor
{
    /// <summary>
    /// Determines whether this extractor can process the specified exception.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns><c>true</c> if this extractor can process the exception; otherwise, <c>false</c>.</returns>
    bool CanExtract(Exception exception);
    
    /// <summary>
    /// Extracts error messages from the specified exception.
    /// </summary>
    /// <param name="exception">The exception to extract errors from.</param>
    /// <returns>A collection of error messages extracted from the exception.</returns>
    IEnumerable<string> ExtractErrors(Exception exception);
}
