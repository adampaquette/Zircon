using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Zircon.AspNetCore.Middlewares;

public sealed partial class DeveloperRequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<DeveloperRequestLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<DeveloperRequestLoggingMiddleware> _logger = logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        await LogRequestDetails(context);

        var originalBodyStream = context.Response.Body;

        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
            await LogResponseDetails(context);

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequestDetails(HttpContext context)
    {
        context.Request.EnableBuffering();

        var requestData = new
        {
            context.Request.Protocol,
            context.Request.Method,
            Url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
            Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = await GetRequestBody(context.Request)
        };

        var requestJson = JsonSerializer.Serialize(requestData, JsonOptions);
        Log.HttpRequestDetails(_logger, requestJson);
    }

    private async Task LogResponseDetails(HttpContext context)
    {
        var responseData = new
        {
            context.Response.StatusCode,
            Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = await GetResponseBody(context.Response)
        };

        var responseJson = JsonSerializer.Serialize(responseData, JsonOptions);
        Log.HttpResponseDetails(_logger, responseJson);
    }

    private static async Task<object?> GetRequestBody(HttpRequest request)
    {
        if (request.ContentLength == 0)
        {
            return null;
        }

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch
        {
            return body;
        }
    }

    private static async Task<object?> GetResponseBody(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(response.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        if (string.IsNullOrEmpty(body))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch
        {
            return body;
        }
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "HTTP Request Details:\n{Request}")]
        public static partial void HttpRequestDetails(ILogger logger, string request);

        [LoggerMessage(2, LogLevel.Information, "HTTP Response Details:\n{Response}")]
        public static partial void HttpResponseDetails(ILogger logger, string response);
    }
}
