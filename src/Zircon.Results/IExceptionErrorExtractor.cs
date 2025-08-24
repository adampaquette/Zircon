namespace Zircon.Results;

public interface IExceptionErrorExtractor
{
    bool CanExtract(Exception exception);
    IEnumerable<string> ExtractErrors(Exception exception);
}
