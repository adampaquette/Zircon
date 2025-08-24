using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Zircon.OpenApi.Swashbuckle.OperationFilters;
using Zircon.OpenApi.Swashbuckle.SchemaFilters;

namespace Zircon.OpenApi.Swashbuckle;

/// <summary>
/// Extension methods for configuring OpenAPI filters in the service collection
/// </summary>
public static class SwaggerGenOptionsExtensions
{
    /// <summary>
    /// Adds all OpenAPI filters to the swagger generation options
    /// </summary>
    /// <param name="options">The swagger gen options</param>
    /// <returns>The swagger gen options for chaining</returns>
    public static SwaggerGenOptions AddOpenApiFilters(this SwaggerGenOptions options)
    {
        return options
            .AddSchemaFilters()
            .AddOperationFilters();
    }

    /// <summary>
    /// Adds schema filters to the swagger generation options
    /// </summary>
    public static SwaggerGenOptions AddSchemaFilters(this SwaggerGenOptions options)
    {
        options.SchemaFilter<EnumSchemaFilter>();
        options.SchemaFilter<RequiredSchemaFilter>();
        return options;
    }

    /// <summary>
    /// Adds operation filters to the swagger generation options
    /// </summary>
    public static SwaggerGenOptions AddOperationFilters(this SwaggerGenOptions options)
    {
        options.OperationFilter<OperationDeprecationFilter>();
        options.OperationFilter<ResponseContentTypeFilter>();
        options.OperationFilter<ParameterDescriptionFilter>();
        options.OperationFilter<ParameterDefaultValueFilter>();
        options.OperationFilter<ParameterRequiredFilter>();
        return options;
    }
}
