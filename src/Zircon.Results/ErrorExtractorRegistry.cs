namespace Zircon.Results;

public class ErrorExtractorRegistry
{
    private readonly HashSet<IExceptionErrorExtractor> _extractors = [];

    public static ErrorExtractorRegistry Instance { get; } = new();

    private ErrorExtractorRegistry() { }

    public ErrorExtractorRegistry Register(IExceptionErrorExtractor extractor)
    {
        ArgumentNullException.ThrowIfNull(extractor);
        _extractors.Add(extractor);
        return this;
    }

    public ErrorExtractorRegistry Unregister(IExceptionErrorExtractor extractor)
    {
        _extractors.Remove(extractor);
        return this;
    }

    public ErrorExtractorRegistry Clear()
    {
        _extractors.Clear();
        return this;
    }

    public IEnumerable<string> ExtractErrors(Exception exception)
    {
        var extractor = _extractors.FirstOrDefault(e => e.CanExtract(exception));
        if (extractor == null)
        {
            return [exception.Message];
        }

        var errors = extractor.ExtractErrors(exception).ToList();
        return errors.Count != 0 ? errors : [exception.Message];
    }
}
