using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Persistence.Json;

internal sealed class CompactInt32ArrayJsonConverter : JsonConverter<int[]>
{
    private readonly int _valuesPerLine;

    public CompactInt32ArrayJsonConverter(int valuesPerLine = 12)
    {
        if (valuesPerLine <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valuesPerLine), valuesPerLine, "Values-per-line must be positive.");
        }

        _valuesPerLine = valuesPerLine;
    }

    public override int[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected {JsonTokenType.StartArray} but got {reader.TokenType}.");
        }

        var values = new List<int>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return values.ToArray();
            }

            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected {JsonTokenType.Number} but got {reader.TokenType}.");
            }

            values.Add(reader.GetInt32());
        }

        throw new JsonException("Unexpected end of JSON while reading an int array.");
    }

    public override void Write(Utf8JsonWriter writer, int[] value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        if (!options.WriteIndented)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
            return;
        }

        var formatted = FormatIntArray(value, writer.CurrentDepth, _valuesPerLine);
        writer.WriteRawValue(formatted, skipInputValidation: true);
    }

    private static string FormatIntArray(int[] value, int currentDepth, int valuesPerLine)
    {
        if (value.Length == 0)
        {
            return "[]";
        }

        var propertyIndent = new string(' ', currentDepth * 2);
        var elementIndent = new string(' ', (currentDepth + 1) * 2);

        var sb = new StringBuilder(capacity: Math.Min(1024, 16 + value.Length * 8));
        sb.Append('[');

        var countOnLine = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (i == 0)
            {
                sb.Append(value[i].ToString(CultureInfo.InvariantCulture));
                countOnLine = 1;
                continue;
            }

            if (countOnLine >= valuesPerLine)
            {
                sb.Append(",\n");
                sb.Append(elementIndent);
                sb.Append(value[i].ToString(CultureInfo.InvariantCulture));
                countOnLine = 1;
            }
            else
            {
                sb.Append(", ");
                sb.Append(value[i].ToString(CultureInfo.InvariantCulture));
                countOnLine++;
            }
        }

        sb.Append('\n');
        sb.Append(propertyIndent);
        sb.Append(']');

        return sb.ToString();
    }
}

internal sealed class CompactDecimalArrayJsonConverter : JsonConverter<decimal[]>
{
    private readonly int _valuesPerLine;

    public CompactDecimalArrayJsonConverter(int valuesPerLine = 12)
    {
        if (valuesPerLine <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valuesPerLine), valuesPerLine, "Values-per-line must be positive.");
        }

        _valuesPerLine = valuesPerLine;
    }

    public override decimal[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected {JsonTokenType.StartArray} but got {reader.TokenType}.");
        }

        var values = new List<decimal>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return values.ToArray();
            }

            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected {JsonTokenType.Number} but got {reader.TokenType}.");
            }

            values.Add(reader.GetDecimal());
        }

        throw new JsonException("Unexpected end of JSON while reading a decimal array.");
    }

    public override void Write(Utf8JsonWriter writer, decimal[] value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        if (!options.WriteIndented)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
            return;
        }

        var formatted = FormatDecimalArray(value, writer.CurrentDepth, _valuesPerLine);
        writer.WriteRawValue(formatted, skipInputValidation: true);
    }

    private static string FormatDecimalArray(decimal[] value, int currentDepth, int valuesPerLine)
    {
        if (value.Length == 0)
        {
            return "[]";
        }

        var propertyIndent = new string(' ', currentDepth * 2);
        var elementIndent = new string(' ', (currentDepth + 1) * 2);

        var sb = new StringBuilder(capacity: Math.Min(1024, 16 + value.Length * 16));
        sb.Append('[');

        var countOnLine = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var text = value[i].ToString("G29", CultureInfo.InvariantCulture);

            if (i == 0)
            {
                sb.Append(text);
                countOnLine = 1;
                continue;
            }

            if (countOnLine >= valuesPerLine)
            {
                sb.Append(",\n");
                sb.Append(elementIndent);
                sb.Append(text);
                countOnLine = 1;
            }
            else
            {
                sb.Append(", ");
                sb.Append(text);
                countOnLine++;
            }
        }

        sb.Append('\n');
        sb.Append(propertyIndent);
        sb.Append(']');

        return sb.ToString();
    }
}
