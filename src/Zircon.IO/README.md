# Zircon.IO

I/O extension methods for streams providing convenient async utilities for reading and manipulating stream data efficiently and safely.

## Features

- **Async Stream Operations**: Asynchronous methods for better performance and scalability
- **Memory Efficient**: Uses MemoryStream for efficient byte array creation
- **Simple API**: Clean, intuitive extension methods
- **Exception Safe**: Proper resource disposal and exception handling
- **Universal Compatibility**: Works with any Stream implementation

## Installation

```bash
dotnet add package Zircon.IO
```

## Usage

### Basic Stream Reading

```csharp
using System.IO;
using Zircon.IO;

// Reading from a file stream
using var fileStream = File.OpenRead("document.pdf");
byte[] fileBytes = await fileStream.ReadAllBytesAsync();

// Reading from HTTP response
using var httpClient = new HttpClient();
using var response = await httpClient.GetAsync("https://example.com/api/data");
using var stream = await response.Content.ReadAsStreamAsync();
byte[] responseBytes = await stream.ReadAllBytesAsync();
```

### Web API File Upload Processing

```csharp
[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");
        
        using var stream = file.OpenReadStream();
        byte[] fileBytes = await stream.ReadAllBytesAsync();
        
        // Process the file bytes
        var result = await _fileProcessingService.ProcessFileAsync(fileBytes, file.FileName);
        
        return Ok(new { FileSize = fileBytes.Length, ProcessedAt = DateTime.UtcNow });
    }
}
```

### Database BLOB Operations

```csharp
public class DocumentService
{
    private readonly IDbConnection _connection;
    
    public async Task<byte[]> GetDocumentContentAsync(int documentId)
    {
        const string sql = "SELECT Content FROM Documents WHERE Id = @Id";
        
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@Id", documentId));
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            using var stream = reader.GetStream("Content");
            return await stream.ReadAllBytesAsync();
        }
        
        throw new DocumentNotFoundException($"Document {documentId} not found");
    }
    
    public async Task SaveDocumentAsync(int documentId, Stream contentStream)
    {
        byte[] content = await contentStream.ReadAllBytesAsync();
        
        const string sql = "UPDATE Documents SET Content = @Content WHERE Id = @Id";
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@Id", documentId));
        command.Parameters.Add(new SqlParameter("@Content", content));
        
        await command.ExecuteNonQueryAsync();
    }
}
```

### Stream Processing Pipeline

```csharp
public class ImageProcessingService
{
    public async Task<byte[]> ProcessImageAsync(Stream inputStream)
    {
        // Read the input image
        byte[] originalBytes = await inputStream.ReadAllBytesAsync();
        
        // Process with image library
        using var image = Image.Load(originalBytes);
        image.Mutate(x => x.Resize(800, 600));
        
        // Convert back to byte array
        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream);
        outputStream.Position = 0;
        
        return await outputStream.ReadAllBytesAsync();
    }
    
    public async Task<ProcessingResult> ProcessMultipleImagesAsync(IEnumerable<Stream> imageStreams)
    {
        var tasks = imageStreams.Select(async stream =>
        {
            var bytes = await stream.ReadAllBytesAsync();
            return new ProcessedImage
            {
                OriginalSize = bytes.Length,
                ProcessedData = await ProcessImageBytesAsync(bytes)
            };
        });
        
        var results = await Task.WhenAll(tasks);
        return new ProcessingResult { Images = results };
    }
}
```

### Caching and Storage

```csharp
public class CacheService
{
    private readonly IMemoryCache _cache;
    
    public async Task<byte[]> GetOrCreateAsync(string key, Func<Task<Stream>> factory)
    {
        if (_cache.TryGetValue(key, out byte[] cached))
        {
            return cached;
        }
        
        using var stream = await factory();
        var bytes = await stream.ReadAllBytesAsync();
        
        _cache.Set(key, bytes, TimeSpan.FromMinutes(30));
        return bytes;
    }
}

// Usage
var pdfBytes = await _cacheService.GetOrCreateAsync("report-2023", async () =>
{
    return await _reportService.GenerateReportStreamAsync(2023);
});
```

### Network Stream Processing

```csharp
public class NetworkFileService
{
    private readonly HttpClient _httpClient;
    
    public async Task<FileDownloadResult> DownloadFileAsync(string url)
    {
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        using var stream = await response.Content.ReadAsStreamAsync();
        var content = await stream.ReadAllBytesAsync();
        
        return new FileDownloadResult
        {
            Content = content,
            ContentType = response.Content.Headers.ContentType?.MediaType,
            FileName = ExtractFileNameFromUrl(url),
            Size = content.Length
        };
    }
    
    public async Task<bool> UploadFileAsync(string url, Stream fileStream)
    {
        var fileBytes = await fileStream.ReadAllBytesAsync();
        
        using var content = new ByteArrayContent(fileBytes);
        using var response = await _httpClient.PostAsync(url, content);
        
        return response.IsSuccessStatusCode;
    }
}
```

## Extension Methods

### ReadAllBytesAsync

```csharp
public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
```

**Description:** Asynchronously reads all bytes from the current stream and returns them as a byte array.

**Parameters:**
- `stream`: The stream to read from

**Returns:**
- `Task<byte[]>`: A task representing the asynchronous operation, containing all bytes from the stream

**Key Features:**
- **Memory Efficient**: Uses MemoryStream internally for optimal memory usage
- **Async/Await Compatible**: Fully asynchronous operation
- **Resource Safe**: Properly disposes of internal resources
- **Position Independent**: Works regardless of current stream position

## Performance Characteristics

- **Memory Usage**: Creates a single MemoryStream internally, minimizing memory allocations
- **Async Operation**: Non-blocking I/O operations for better scalability
- **Buffer Management**: Leverages Stream.CopyToAsync for optimal buffering
- **GC Pressure**: Minimal garbage collection pressure due to efficient implementation

## Comparison with Alternatives

| Method | Memory Efficiency | Async Support | Code Simplicity |
|--------|------------------|---------------|-----------------|
| `stream.ReadAllBytesAsync()` | High | Yes | Excellent |
| Manual byte[] + Read loop | Medium | Can be added | Poor |
| ReadToEnd + Encoding | Low | Yes | Good |
| Multiple Read calls | Low | Can be added | Poor |

## Error Handling

```csharp
public async Task<byte[]> SafeReadAllBytesAsync(Stream stream)
{
    try
    {
        return await stream.ReadAllBytesAsync();
    }
    catch (IOException ioEx)
    {
        _logger.LogError(ioEx, "I/O error while reading stream");
        throw new StreamProcessingException("Failed to read stream due to I/O error", ioEx);
    }
    catch (ObjectDisposedException)
    {
        _logger.LogWarning("Attempted to read from disposed stream");
        throw new InvalidOperationException("Cannot read from disposed stream");
    }
    catch (NotSupportedException)
    {
        _logger.LogWarning("Stream does not support reading");
        throw new InvalidOperationException("Stream does not support reading");
    }
}
```

## Integration with Dependency Injection

```csharp
public interface IStreamProcessor
{
    Task<byte[]> ProcessStreamAsync(Stream input);
}

public class StreamProcessor : IStreamProcessor
{
    private readonly ILogger<StreamProcessor> _logger;
    
    public StreamProcessor(ILogger<StreamProcessor> logger)
    {
        _logger = logger;
    }
    
    public async Task<byte[]> ProcessStreamAsync(Stream input)
    {
        _logger.LogInformation("Processing stream of length: {Length}", input.Length);
        
        var bytes = await input.ReadAllBytesAsync();
        
        _logger.LogInformation("Successfully processed {ByteCount} bytes", bytes.Length);
        return bytes;
    }
}

// Registration
services.AddScoped<IStreamProcessor, StreamProcessor>();
```

## Best Practices

1. **Resource Management**: Always use `using` statements with streams
2. **Exception Handling**: Wrap in try-catch for production scenarios
3. **Memory Considerations**: Be mindful of large files and available memory
4. **Stream Position**: The method reads from current position to end
5. **Async Patterns**: Use ConfigureAwait(false) in library code if needed

## Advanced Scenarios

### Streaming Large Files

```csharp
public async Task<ProcessingResult> ProcessLargeFileAsync(Stream largeFileStream, int maxSize = 100_000_000)
{
    if (largeFileStream.Length > maxSize)
    {
        throw new FileTooLargeException($"File size {largeFileStream.Length} exceeds maximum {maxSize}");
    }
    
    var bytes = await largeFileStream.ReadAllBytesAsync();
    return await ProcessBytesInChunks(bytes);
}
```

### Combining with Other Extensions

```csharp
public static async Task<string> ReadAllTextAsync(this Stream stream, Encoding encoding = null)
{
    var bytes = await stream.ReadAllBytesAsync();
    return (encoding ?? Encoding.UTF8).GetString(bytes);
}

public static async Task<T> ReadJsonAsync<T>(this Stream stream)
{
    var bytes = await stream.ReadAllBytesAsync();
    return JsonSerializer.Deserialize<T>(bytes);
}
```

## Future Extensions

This package provides a foundation for additional I/O utilities. Future versions may include:

- Stream writing utilities
- Compression/decompression extensions
- Encryption/decryption helpers
- Progress reporting for large operations
- Chunked reading operations

## License

This project is licensed under the MIT License - see the LICENSE file for details.