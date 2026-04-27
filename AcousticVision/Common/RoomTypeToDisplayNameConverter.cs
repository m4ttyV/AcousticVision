using System;
using System.Globalization;
using AcousticVision.Models;
using Avalonia.Data.Converters;

namespace AcousticVision.Common;

public sealed class RoomTypeToDisplayNameConverter : IValueConverter
{
    public static readonly RoomTypeToDisplayNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is RoomType roomType
            ? roomType.ToDisplayName()
            : string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}