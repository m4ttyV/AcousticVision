using AcousticVision.Common;
using AcousticVision.Data;
using AcousticVision.Models;
using AcousticVision.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class RoomPreviewService
{
    private readonly AppDbContext _dbContext;

    public RoomPreviewService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomPreviewViewModel> BuildForTestModelAsync(int testModelId)
    {
        var testModel = await _dbContext.TestModels
            .Include(x => x.Room)
            .Include(x => x.Source)
            .Include(x => x.Receiver)
            .FirstOrDefaultAsync(x => x.Id == testModelId);

        if (testModel is null || testModel.Room is null)
            return Empty("Не выбрана корректная тестовая модель.");

        var room = testModel.Room;

        if (room.Length <= 0 || room.Width <= 0)
            return Empty("Размеры помещения заданы некорректно.");

        var surfaces = await _dbContext.RoomSurfaces
            .Include(x => x.Material)
            .Include(x => x.Texture)
            .Where(x => x.RoomId == room.Id)
            .ToListAsync();

        var surfaceMap = surfaces
            .GroupBy(x => x.Position.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        var north = GetSurface(surfaceMap, "north", "Северная стена");
        var south = GetSurface(surfaceMap, "south", "Южная стена");
        var east = GetSurface(surfaceMap, "east", "Восточная стена");
        var west = GetSurface(surfaceMap, "west", "Западная стена");
        var floor = GetSurface(surfaceMap, "floor", "Пол");
        var ceiling = GetSurface(surfaceMap, "ceiling", "Потолок");

        const double canvasWidth = 360;
        const double canvasHeight = 240;
        const double padding = 26;
        const double wallThickness = 14;
        const double markerSize = 16;

        var scaleX = (canvasWidth - 2 * padding) / room.Length;
        var scaleY = (canvasHeight - 2 * padding) / room.Width;
        var scale = Math.Min(scaleX, scaleY);

        var roomWidth = room.Length * scale;
        var roomHeight = room.Width * scale;
        var roomX = (canvasWidth - roomWidth) / 2;
        var roomY = (canvasHeight - roomHeight) / 2;

        var sourcePoint = CoordinateHelper.TryParsePoint3D(testModel.SourceLocation, out var sp)
            ? sp
            : new Point3D(0, 0, 0);

        var receiverPoint = CoordinateHelper.TryParsePoint3D(testModel.ReceiverLocation, out var rp)
            ? rp
            : new Point3D(0, 0, 0);

        var sourceX = roomX + (sourcePoint.X / room.Length) * roomWidth - markerSize / 2;
        var sourceY = roomY + ((room.Width - sourcePoint.Y) / room.Width) * roomHeight - markerSize / 2;

        var receiverX = roomX + (receiverPoint.X / room.Length) * roomWidth - markerSize / 2;
        var receiverY = roomY + ((room.Width - receiverPoint.Y) / room.Width) * roomHeight - markerSize / 2;

        return new RoomPreviewViewModel
        {
            IsAvailable = true,
            Title = $"Схема помещения: {room.Name}",
            Summary = $"Тип: {room.RoomTypeDisplayName} · Размеры: {room.Length:F1} × {room.Width:F1} × {room.Height:F1} м",
            CanvasWidth = canvasWidth,
            CanvasHeight = canvasHeight,
            RoomX = roomX,
            RoomY = roomY,
            RoomWidth = roomWidth,
            RoomHeight = roomHeight,
            WallThickness = wallThickness,
            MarkerSize = markerSize,
            NorthBrush = AbsorptionColorHelper.GetBrush(north.Absorption),
            SouthBrush = AbsorptionColorHelper.GetBrush(south.Absorption),
            EastBrush = AbsorptionColorHelper.GetBrush(east.Absorption),
            WestBrush = AbsorptionColorHelper.GetBrush(west.Absorption),
            FloorBrush = AbsorptionColorHelper.GetBrush(floor.Absorption),
            CeilingBrush = AbsorptionColorHelper.GetBrush(ceiling.Absorption),
            NorthLabel = AbsorptionColorHelper.FormatLabel(north.PositionName, north.MaterialName, north.Absorption),
            SouthLabel = AbsorptionColorHelper.FormatLabel(south.PositionName, south.MaterialName, south.Absorption),
            EastLabel = AbsorptionColorHelper.FormatLabel(east.PositionName, east.MaterialName, east.Absorption),
            WestLabel = AbsorptionColorHelper.FormatLabel(west.PositionName, west.MaterialName, west.Absorption),
            FloorLabel = AbsorptionColorHelper.FormatLabel(floor.PositionName, floor.MaterialName, floor.Absorption),
            CeilingLabel = AbsorptionColorHelper.FormatLabel(ceiling.PositionName, ceiling.MaterialName, ceiling.Absorption),
            SourceX = sourceX,
            SourceY = sourceY,
            ReceiverX = receiverX,
            ReceiverY = receiverY,
            SourceInfo = $"Источник: {testModel.Source?.Name ?? "—"}, {testModel.SourceLocation}",
            ReceiverInfo = $"Приёмник: {testModel.Receiver?.Name ?? "—"}, {testModel.ReceiverLocation}"
        };
    }

    private static RoomPreviewViewModel Empty(string message)
    {
        return new RoomPreviewViewModel
        {
            IsAvailable = false,
            Title = "Схема помещения",
            Summary = message
        };
    }

    private static SurfaceDescriptor GetSurface(Dictionary<string, RoomSurface> surfaceMap, string key, string positionName)
    {
        if (!surfaceMap.TryGetValue(key, out var surface) || surface.Material is null)
        {
            return new SurfaceDescriptor(positionName, null, null);
        }

        return new SurfaceDescriptor(
            positionName,
            surface.Material.Name,
            AcousticCalculationHelper.GetEffectiveAbsorption(surface));
    }

    private sealed record SurfaceDescriptor(string PositionName, string? MaterialName, double? Absorption);
}
