using AcousticVision.Common;
using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class AnalysisService
{
    private readonly AppDbContext _dbContext;

    public AnalysisService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AnalysisResult> AnalyzeAsync(int testModelId)
    {
        var testModel = await _dbContext.TestModels
            .Include(x => x.Room)
            .Include(x => x.Source)
            .Include(x => x.Receiver)
            .FirstOrDefaultAsync(x => x.Id == testModelId);

        if (testModel is null)
            return Fail("Тестовая модель не найдена.");

        if (testModel.Room is null)
            return Fail("У тестовой модели отсутствует помещение.");

        if (testModel.Source is null)
            return Fail("У тестовой модели отсутствует источник звука.");

        if (testModel.Receiver is null)
            return Fail("У тестовой модели отсутствует приёмник звука.");

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
            .Include(x => x.Texture)
            .Where(x => x.RoomId == room.Id)
            .ToListAsync();

        if (surfaces.Count == 0)
            return Fail("Для помещения не заданы поверхности. Сначала заполните раздел «Поверхности помещения».");

        var requiredPositions = new[] { "floor", "ceiling", "north", "south", "east", "west" };

        var existingPositions = surfaces
            .Select(x => x.Position?.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var missingPositions = requiredPositions
            .Where(x => !existingPositions.Contains(x))
            .ToList();

        if (missingPositions.Count > 0)
            return Fail($"Модель помещения неполная. Не заданы позиции: {string.Join(", ", missingPositions)}.");

        var volume = room.Length * room.Width * room.Height;

        double equivalentAbsorptionArea = 0;

        foreach (var surface in surfaces)
        {
            var normalizedPosition = surface.Position.Trim().ToLowerInvariant();
            var area = GetSurfaceArea(room, normalizedPosition);

            if (area <= 0)
                continue;

            // Пока в RT60 участвует только материал.
            var alpha = Clamp01(surface.Material?.NoiseCancelation ?? 0.0);

            equivalentAbsorptionArea += area * alpha;
        }

        if (equivalentAbsorptionArea <= 0)
            return Fail("Суммарное эквивалентное звукопоглощение оказалось нулевым.");

        // Формула Сабина
        var rt60 = 0.161 * volume / equivalentAbsorptionArea;

        // Локальный расчёт по координатам
        var distance = CoordinateHelper.Distance(sourcePoint, receiverPoint);
        var effectiveDistance = Math.Max(distance, 0.5);

        // Ослабление прямого сигнала с расстоянием
        var attenuationDb = 20.0 * Math.Log10(effectiveDistance);

        // Условный уровень сигнала в точке приёмника
        var sourceLevel = testModel.Source.Volume;
        var estimatedDirectLevelDb = sourceLevel - attenuationDb;

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
            DistanceAttenuationDb = attenuationDb,
            EstimatedDirectLevelDb = estimatedDirectLevelDb,
            Recommendation = BuildRecommendation(rt60, distance, estimatedDirectLevelDb)
        };
    }

    private static double GetSurfaceArea(RoomModel room, string position)
    {
        return position switch
        {
            "floor" => room.Length * room.Width,
            "ceiling" => room.Length * room.Width,
            "north" => room.Length * room.Height,
            "south" => room.Length * room.Height,
            "east" => room.Width * room.Height,
            "west" => room.Width * room.Height,
            _ => 0
        };
    }

    private static double Clamp01(double value)
    {
        if (value < 0) return 0;
        if (value > 1) return 1;
        return value;
    }

    private static AnalysisResult Fail(string message)
    {
        return new AnalysisResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    private static string BuildRecommendation(double rt60, double distance, double directLevelDb)
    {
        var rtComment = GetRtComment(rt60);
        var distanceComment = GetDistanceComment(distance);
        var levelComment = GetDirectLevelComment(directLevelDb);

        return $"{rtComment} {distanceComment} {levelComment}";
    }

    private static string GetRtComment(double rt60)
    {
        if (rt60 < 0.5)
        {
            return "Время реверберации ниже рекомендуемого диапазона для речевой связи. Помещение может восприниматься как акустически «сухое», однако разборчивость речи, вероятно, останется высокой.";
        }

        if (rt60 <= 1.0)
        {
            return "Время реверберации находится в рекомендуемом диапазоне для восприятия речи. Акустические условия помещения в целом можно считать благоприятными.";
        }

        if (rt60 <= 1.5)
        {
            return "Время реверберации превышает оптимальный диапазон. В таких условиях речь может восприниматься менее чётко, особенно при увеличении расстояния между источником и приёмником.";
        }

        return "Время реверберации существенно превышает рекомендуемые значения. Это указывает на повышенную гулкость помещения и вероятное снижение разборчивости речи.";
    }

    private static string GetDistanceComment(double distance)
    {
        if (distance < 1.0)
        {
            return "Источник и приёмник расположены очень близко друг к другу, поэтому влияние расстояния на ослабление прямого сигнала минимально.";
        }

        if (distance <= 3.0)
        {
            return "Расстояние между источником и приёмником можно считать умеренным; при такой конфигурации прямой сигнал обычно сохраняет достаточный уровень.";
        }

        if (distance <= 5.0)
        {
            return "Расстояние между источником и приёмником заметно увеличено, поэтому вклад прямого сигнала в точке прослушивания снижается.";
        }

        return "Источник и приёмник расположены на значительном расстоянии, что может привести к заметному ослаблению прямого сигнала в точке приёмника.";
    }

    private static string GetDirectLevelComment(double directLevelDb)
    {
        if (directLevelDb < 45)
        {
            return "Оценочный уровень прямого сигнала в точке приёмника является низким; для повышения качества восприятия речи целесообразно уменьшить расстояние между источником и приёмником либо улучшить акустические свойства помещения.";
        }

        if (directLevelDb < 55)
        {
            return "Оценочный уровень прямого сигнала можно считать удовлетворительным, однако в неблагоприятных акустических условиях он может оказаться недостаточным для комфортного восприятия речи.";
        }

        return "Оценочный уровень прямого сигнала в точке приёмника является достаточным, что положительно сказывается на восприятии речевого сообщения.";
    }
}