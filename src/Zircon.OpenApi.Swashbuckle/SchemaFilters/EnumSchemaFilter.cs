using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zircon.OpenApi.Swashbuckle.SchemaFilters;

/// <summary>
/// Filter to convert enum types to string-based enums in OpenAPI documentation
/// </summary>
public sealed class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Type.IsEnum)
        {
            return;
        }

        schema.Enum.Clear();
        schema.Type = "string";
        schema.Format = null;

        var enumNames = Enum.GetNames(context.Type);
        foreach (var name in enumNames)
        {
            schema.Enum.Add(new OpenApiString(name));
        }
    }
}
