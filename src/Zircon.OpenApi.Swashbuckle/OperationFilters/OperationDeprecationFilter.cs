using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zircon.OpenApi.Swashbuckle.OperationFilters;

/// <summary>
/// Filter to handle operation deprecation in OpenAPI documentation
/// </summary>
internal sealed class OperationDeprecationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        operation.Deprecated |= context.ApiDescription.IsDeprecated();
    }
}
