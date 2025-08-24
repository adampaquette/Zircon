using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zircon.OpenApi.Swashbuckle.OperationFilters;

/// <summary>
/// Filter to enrich parameters with their default values
/// </summary>
internal sealed class ParameterDefaultValueFilter(ILogger<ParameterDefaultValueFilter> logger) : IOperationFilter
{
    private readonly ILogger<ParameterDefaultValueFilter> _logger = logger;

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
            var description = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Name == parameter.Name);

            if (description == null)
            {
                continue;
            }

            UpdateParameterDefaultValue(parameter, description);
        }
    }

    private void UpdateParameterDefaultValue(OpenApiParameter parameter, ApiParameterDescription description)
    {
        if (parameter.Schema.Default != null ||
            description.DefaultValue == null ||
            description.DefaultValue is DBNull ||
            description.ModelMetadata is not ModelMetadata modelMetadata)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(description.DefaultValue, modelMetadata.ModelType);
            parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex,
                "Failed to serialize default value for parameter {ParameterName}",
                parameter.Name);
        }
    }
}
