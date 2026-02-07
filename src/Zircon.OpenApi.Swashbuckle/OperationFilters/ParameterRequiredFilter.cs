using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zircon.OpenApi.Swashbuckle.OperationFilters;

/// <summary>
/// Filter to handle required parameters
/// </summary>
internal sealed class ParameterRequiredFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (operation.Parameters == null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            if (parameter is not OpenApiParameter concreteParameter)
            {
                continue;
            }

            var description = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Name == concreteParameter.Name);

            if (description != null)
            {
                concreteParameter.Required |= description.IsRequired;
            }
        }
    }
}
