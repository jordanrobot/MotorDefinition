using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SkiaSharp;

namespace CurveEditor.Converters;

/// <summary>
/// Converts an SKColor to an Avalonia Brush.
/// </summary>
public sealed class SKColorToAvaloniaBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SKColor skColor)
        {
            return new SolidColorBrush(Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue));
        }

        return new SolidColorBrush(Colors.Black);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
