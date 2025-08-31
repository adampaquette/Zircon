namespace Zircon.IO;

/// <summary>
/// Extension methods for <see cref="Stream"/> to provide enhanced I/O capabilities.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Asynchronously reads all bytes from the current stream and returns them as a byte array.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>A task that represents the asynchronous read operation. The value of the task contains a byte array with all the bytes from the stream.</returns>
    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}
