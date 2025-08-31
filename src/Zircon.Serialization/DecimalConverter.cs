using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zircon.Serialization;

/// <summary>
/// A custom JSON converter for nullable decimal values that handles culture-specific formatting
/// and provides robust parsing for both numeric and string representations.
/// </summary>
public class DecimalConverter : JsonConverter<decimal?>
{
    /// <summary>
    /// Reads and converts the JSON to a nullable decimal value.
    /// Supports null values, numeric tokens, and string representations with comma-to-dot conversion.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted decimal value or null.</returns>
    /// <exception cref="JsonException">Thrown when the token cannot be converted to a decimal.</exception>
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.Number:
                return reader.GetDecimal();
            case JsonTokenType.String:
                {
                    string? value = reader.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return null;
                    }

                    value = value.Replace(',', '.');

                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    {
                        return result;
                    }

                    break;
                }
        }

        throw new JsonException($"Impossible de convertir en decimal. Token: {reader.TokenType}");
    }

    /// <summary>
    /// Writes the decimal value to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The decimal value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
