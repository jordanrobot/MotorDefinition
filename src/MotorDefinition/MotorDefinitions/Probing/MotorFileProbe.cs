using JordanRobot.MotorDefinitions.Model;
using System;
using System.Linq;
using System.Text.Json;

namespace JordanRobot.MotorDefinitions.Persistence.Probing;

/// <summary>
/// Lightweight shape probe for motor definition files to avoid full deserialization.
/// </summary>
internal static class MotorFileProbe
{
    /// <summary>
    /// Determines whether a <see cref="JsonDocument"/> resembles a motor definition file in the series table/map format.
    /// </summary>
    /// <param name="document">The parsed JSON document.</param>
    /// <returns>True if the document matches the expected shape; otherwise false.</returns>
    public static bool IsLikelyMotorDefinition(JsonDocument document)
    {
        try
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!TryGetString(root, "schemaVersion", out var schemaVersion) ||
                !string.Equals(schemaVersion, MotorDefinition.CurrentSchemaVersion, StringComparison.Ordinal))
            {
                return false;
            }

            if (!TryGetString(root, "motorName", out var motorName) || string.IsNullOrWhiteSpace(motorName))
            {
                return false;
            }

            if (!root.TryGetProperty("drives", out var drivesElement) || drivesElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var anyValidVoltage = false;
            foreach (var drive in drivesElement.EnumerateArray())
            {
                if (!drive.TryGetProperty("voltages", out var voltagesElement) || voltagesElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var voltage in voltagesElement.EnumerateArray())
                {
                    if (!HasAxis(voltage, "percent") || !HasAxis(voltage, "rpm"))
                    {
                        continue;
                    }

                    if (!voltage.TryGetProperty("series", out var seriesElement) || seriesElement.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    if (!seriesElement.EnumerateObject().Any())
                    {
                        continue;
                    }

                    anyValidVoltage = true;
                    break;
                }

                if (anyValidVoltage)
                {
                    break;
                }
            }

            return anyValidVoltage;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string? value)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return true;
        }

        value = null;
        return false;
    }

    private static bool HasAxis(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var axis))
        {
            return false;
        }

        if (axis.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        // Length is variable (including 0). This probe only verifies presence and shape.
        return true;
    }
}
