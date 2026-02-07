using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zircon.OpenApi.Swashbuckle.SchemaFilters;

/// <summary>
/// Filter to convert enum types to string-based enums in OpenAPI documentation
/// </summary>
public sealed class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Type.IsEnum)
        {
            return;
        }

        if (schema is not OpenApiSchema concreteSchema)
        {
            return;
        }

        concreteSchema.Enum?.Clear();
        concreteSchema.Type = JsonSchemaType.String;
        concreteSchema.Format = null;

        var enumNames = Enum.GetNames(context.Type);
        foreach (var name in enumNames)
        {
            concreteSchema.Enum?.Add(JsonValue.Create(name));
        }
    }
}
