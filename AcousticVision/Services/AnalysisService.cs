using AcousticVision.Common;
using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class AnalysisService
{
    private readonly AppDbContext _dbContext;

    public AnalysisService(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<AnalysisResult> AnalyzeAsync(int testModelId)
    {
        var testModel = await _dbContext.TestModels
            .Include(x => x.Room)
            .Include(x => x.Source)
            .Include(x => x.Receiver)
            .FirstOrDefaultAsync(x => x.Id == testModelId);

        if (testModel is null) return Fail("Тестовая модель не найдена.");
        if (testModel.Room is null) return Fail("У тестовой модели отсутствует помещение.");
        if (testModel.Source is null) return Fail("У тестовой модели отсутствует источник звука.");
        if (testModel.Receiver is null) return Fail("У тестовой модели отсутствует приёмник звука.");

        var room = testModel.Room;
        if (room.Length <= 0 || room.Width <= 0 || room.Height <= 0)
            return Fail("Размеры помещения должны быть больше нуля.");

        if (!CoordinateHelper.TryParsePoint3D(testModel.SourceLocation, out var sourcePoint))
            return Fail("Координаты источника в тестовой модели заданы некорректно.");

        if (!CoordinateHelper.TryParsePoint3D(testModel.ReceiverLocation, out var receiverPoint))
            return Fail("Координаты приёмника в тестовой модели заданы некорректно.");

        if (!CoordinateHelper.IsInsideRoom(sourcePoint, room.Length, room.Width, room.Height))
            return Fail("Координаты источника выходят за границы помещения.");

        if (!CoordinateHelper.IsInsideRoom(receiverPoint, room.Length, room.Width, room.Height))
            return Fail("Координаты приёмника выходят за границы помещения.");

        var surfaces = await _dbContext.RoomSurfaces
            .Include(x => x.Material)
            .Where(x => x.RoomId == room.Id)
            .ToListAsync();

        if (surfaces.Count == 0)
            return Fail("Для помещения не заданы поверхности. Сначала заполните раздел «Поверхности помещения».");

        var requiredPositions = new[] { "floor", "ceiling", "north", "south", "east", "west" };
        var existingPositions = surfaces.Select(x => x.Position.Trim().ToLowerInvariant()).Distinct().ToList();
        var missingPositions = requiredPositions.Where(x => !existingPositions.Contains(x)).ToList();

        if (missingPositions.Count > 0)
            return Fail($"Модель помещения неполная. Не заданы позиции: {string.Join(", ", missingPositions)}.");

        var volume = room.Length * room.Width * room.Height;
        double equivalentAbsorptionArea = 0;

        foreach (var surface in surfaces)
        {
            var area = GetSurfaceArea(room, surface.Position.Trim().ToLowerInvariant());
            var alpha = Clamp01(surface.Material?.NoiseCancelation ?? 0.0);
            equivalentAbsorptionArea += area * alpha;
        }

        if (equivalentAbsorptionArea <= 0)
            return Fail("Суммарное эквивалентное звукопоглощение оказалось нулевым.");

        var rt60 = 0.161 * volume / equivalentAbsorptionArea;
        var distance = CoordinateHelper.Distance(sourcePoint, receiverPoint);

        return new AnalysisResult
        {
            IsSuccess = true,
            Message = "Расчёт успешно выполнен.",
            TestModelId = testModel.Id,
            RoomName = testModel.Room.Name,
            SourceName = testModel.Source.Name,
            ReceiverName = testModel.Receiver.Name,
            SourceLocation = testModel.SourceLocation,
            ReceiverLocation = testModel.ReceiverLocation,
            Volume = volume,
            EquivalentAbsorptionArea = equivalentAbsorptionArea,
            Rt60 = rt60,
            SourceReceiverDistance = distance,
            Recommendation = BuildRecommendation(rt60, distance)
        };
    }

    private static double GetSurfaceArea(RoomModel room, string position) => position switch
    {
        "floor" => room.Length * room.Width,
        "ceiling" => room.Length * room.Width,
        "north" => room.Length * room.Height,
        "south" => room.Length * room.Height,
        "east" => room.Width * room.Height,
        "west" => room.Width * room.Height,
        _ => 0
    };

    private static double Clamp01(double value) => value < 0 ? 0 : value > 1 ? 1 : value;

    private static AnalysisResult Fail(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };

    private static string BuildRecommendation(double rt60, double distance)
    {
        var rtText = rt60 switch
        {
            < 0.5 => "Время реверберации низкое. Помещение может быть избыточно заглушено для естественного звучания речи.",
            <= 1.0 => "Время реверберации находится в рекомендуемом диапазоне для речевой связи.",
            <= 1.5 => "Разборчивость речи может снижаться. Желательно увеличить звукопоглощение потолка или стен.",
            _ => "Время реверберации слишком велико. Требуется существенно повысить звукопоглощение и уменьшить поздние отражения."
        };

        var distanceText = distance > 5
            ? " Расстояние между источником и приёмником достаточно велико, поэтому при дальнейшем развитии модели стоит учитывать ослабление прямого сигнала."
            : string.Empty;

        return rtText + distanceText;
    }
}
