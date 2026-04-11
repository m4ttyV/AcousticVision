namespace AcousticVision.Common;

public readonly record struct Point3D(double X, double Y, double Z);

public static class CoordinateHelper
{
    public static bool TryParsePoint3D(string? input, out Point3D point)
    {
        point = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var text = input.Trim();

        if (text.StartsWith("("))
            text = text[1..];
        if (text.EndsWith(")"))
            text = text[..^1];

        var parts = text.Split(';', System.StringSplitOptions.TrimEntries | System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        if (!TryParseDouble(parts[0], out var x))
            return false;
        if (!TryParseDouble(parts[1], out var y))
            return false;
        if (!TryParseDouble(parts[2], out var z))
            return false;

        point = new Point3D(x, y, z);
        return true;
    }

    public static bool IsInsideRoom(Point3D point, double length, double width, double height)
    {
        return point.X >= 0 && point.X <= length
            && point.Y >= 0 && point.Y <= width
            && point.Z >= 0 && point.Z <= height;
    }

    public static double Distance(Point3D a, Point3D b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static bool TryParseDouble(string input, out double value)
    {
        var normalized = input.Trim().Replace(',', '.');
        return double.TryParse(normalized, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value);
    }
}
