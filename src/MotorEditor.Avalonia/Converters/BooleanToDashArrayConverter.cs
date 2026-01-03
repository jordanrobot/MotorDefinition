using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Collections;

namespace CurveEditor.Converters;

/// <summary>
/// Converts a boolean isDashed value to a stroke dash array.
/// </summary>
public sealed class BooleanToDashArrayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isDashed && isDashed)
        {
            return new AvaloniaList<double> { 5, 5 };
        }

        return null; // Solid line
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
