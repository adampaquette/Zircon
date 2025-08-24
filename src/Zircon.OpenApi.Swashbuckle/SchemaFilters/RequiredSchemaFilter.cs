using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zircon.OpenApi.Swashbuckle.SchemaFilters;

/// <summary>
/// Filter to mark non-nullable value type properties as required in OpenAPI documentation
/// </summary>
public sealed class RequiredSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (context.Type == null || !schema.Properties.Any())
        {
            return;
        }

        var requiredProperties = context.Type.GetProperties()
            .Where(IsRequiredProperty)
            .Select(GetJsonPropertyName);

        foreach (var propertyName in requiredProperties)
        {
            if (schema.Properties.ContainsKey(propertyName))
            {
                schema.Required ??= new HashSet<string>();
                schema.Required.Add(propertyName);
            }
        }
    }

    private static bool IsRequiredProperty(PropertyInfo property) =>
        Nullable.GetUnderlyingType(property.PropertyType) == null &&
        property.PropertyType != typeof(string) &&
        !property.PropertyType.IsClass;

    private static string GetJsonPropertyName(PropertyInfo property)
    {
        // Check for JsonPropertyName attribute
        var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        if (jsonPropertyAttribute != null)
        {
            return jsonPropertyAttribute.Name;
        }

        // Default camelCase naming
        return char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
    }
}
