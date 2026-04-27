using Avalonia.Media;

namespace AcousticVision.Common;

public static class AbsorptionColorHelper
{
    private static readonly Color LowColor = Color.Parse("#DCEBFA");
    private static readonly Color HighColor = Color.Parse("#1E5FAF");
    private static readonly Color MissingColor = Color.Parse("#CBD5E1");

    public static IBrush GetBrush(double? absorption)
    {
        if (absorption is null)
            return new SolidColorBrush(MissingColor);

        var alpha = Clamp01(absorption.Value);

        var color = Interpolate(LowColor, HighColor, alpha);
        return new SolidColorBrush(color);
    }

    public static string FormatLabel(string positionName, string? materialName, double? absorption)
    {
        if (string.IsNullOrWhiteSpace(materialName) || absorption is null)
            return $"{positionName}: не задано";

        return $"{positionName}: {materialName} (α={absorption.Value:F2})";
    }

    private static double Clamp01(double value)
    {
        if (value < 0) return 0;
        if (value > 1) return 1;
        return value;
    }

    private static Color Interpolate(Color start, Color end, double t)
    {
        byte Lerp(byte a, byte b) => (byte)(a + (b - a) * t);

        return Color.FromArgb(
            255,
            Lerp(start.R, end.R),
            Lerp(start.G, end.G),
            Lerp(start.B, end.B));
    }
}
