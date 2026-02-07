using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zircon.OpenApi.Swashbuckle.OperationFilters;

/// <summary>
/// Filter to clean up unsupported content types in responses
/// </summary>
internal sealed class ResponseContentTypeFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();

            if (!operation.Responses.TryGetValue(responseKey, out var response))
            {
                continue;
            }

            if (response.Content == null)
            {
                continue;
            }

            var unsupportedTypes = response.Content.Keys
                .Where(contentType => responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                .ToList();

            foreach (var unsupportedType in unsupportedTypes)
            {
                response.Content.Remove(unsupportedType);
            }
        }
    }
}
