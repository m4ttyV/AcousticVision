using Avalonia.Media;

namespace AcousticVision.ViewModels;

public sealed class RoomPreviewViewModel
{
    public bool IsAvailable { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;

    public double CanvasWidth { get; init; } = 360;
    public double CanvasHeight { get; init; } = 240;

    public double RoomX { get; init; }
    public double RoomY { get; init; }
    public double RoomWidth { get; init; }
    public double RoomHeight { get; init; }
    public double WallThickness { get; init; } = 14;

    public double SouthWallY => RoomY + RoomHeight - WallThickness;
    public double EastWallX => RoomX + RoomWidth - WallThickness;
    public double SouthLabelY => RoomY + RoomHeight + 6;
    public double EastLabelX => RoomX + RoomWidth + 6;

    public IBrush NorthBrush { get; init; } = Brushes.LightGray;
    public IBrush SouthBrush { get; init; } = Brushes.LightGray;
    public IBrush EastBrush { get; init; } = Brushes.LightGray;
    public IBrush WestBrush { get; init; } = Brushes.LightGray;
    public IBrush FloorBrush { get; init; } = Brushes.LightGray;
    public IBrush CeilingBrush { get; init; } = Brushes.LightGray;

    public string NorthLabel { get; init; } = string.Empty;
    public string SouthLabel { get; init; } = string.Empty;
    public string EastLabel { get; init; } = string.Empty;
    public string WestLabel { get; init; } = string.Empty;
    public string FloorLabel { get; init; } = string.Empty;
    public string CeilingLabel { get; init; } = string.Empty;

    public double SourceX { get; init; }
    public double SourceY { get; init; }
    public double ReceiverX { get; init; }
    public double ReceiverY { get; init; }
    public double MarkerSize { get; init; } = 16;

    public string SourceInfo { get; init; } = string.Empty;
    public string ReceiverInfo { get; init; } = string.Empty;
    public string LegendText { get; init; } = "Светлее — меньшее звукопоглощение; темнее — большее звукопоглощение.";
}
