namespace AcousticVision.Common;

public static class SurfacePositionExtensions
{
    public static string ToDisplayName(this string? position)
    {
        return position?.Trim().ToLowerInvariant() switch
        {
            "north" => "Северная стена",
            "south" => "Южная стена",
            "east" => "Восточная стена",
            "west" => "Западная стена",
            "floor" => "Пол",
            "ceiling" => "Потолок",
            _ => position ?? string.Empty
        };
    }
}