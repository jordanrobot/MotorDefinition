using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace CurveEditor.Converters;

/// <summary>
/// Converts a file path to just the filename (without directory).
/// </summary>
public sealed class FilePathToNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string filePath && !string.IsNullOrWhiteSpace(filePath))
        {
            try
            {
                return Path.GetFileName(filePath);
            }
            catch
            {
                return filePath;
            }
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("FilePathToNameConverter does not support ConvertBack.");
    }
}
