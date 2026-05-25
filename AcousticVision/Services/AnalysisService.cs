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
        double totalSurfaceArea = 0;

        foreach (var surface in surfaces)
        {
            var normalizedPosition = surface.Position.Trim().ToLowerInvariant();
            var area = GetSurfaceArea(room, normalizedPosition);

            if (area <= 0)
                continue;

            // Эффективный коэффициент учитывает материал и корректирующее влияние фактуры:
            // alpha_eff = Clamp(alpha_material + 0.3 * alpha_texture, 0, 1).
            var alpha = AcousticCalculationHelper.GetEffectiveAbsorption(surface);

            equivalentAbsorptionArea += area * alpha;
            totalSurfaceArea += area;
        }

        if (equivalentAbsorptionArea <= 0)
            return Fail("Суммарное эквивалентное звукопоглощение оказалось нулевым.");

        if (totalSurfaceArea <= 0)
            return Fail("Суммарная площадь поверхностей оказалась нулевой.");

        var averageAbsorption = AcousticCalculationHelper.Clamp01(equivalentAbsorptionArea / totalSurfaceArea);

        var resolvedMethod = ResolveAnalysisMethod(testModel.AnalysisMethod, averageAbsorption);

        var rt60 = resolvedMethod switch
        {
            AnalysisMethod.Sabine => CalculateSabine(volume, equivalentAbsorptionArea),
            AnalysisMethod.Eyring => CalculateEyring(volume, totalSurfaceArea, averageAbsorption),
            _ => CalculateSabine(volume, equivalentAbsorptionArea)
        };

        var roomRequirement = RoomTypeRequirements.Get(room.RoomType);

        var distance = CoordinateHelper.Distance(sourcePoint, receiverPoint);
        var effectiveDistance = Math.Max(distance, 0.5);

        var attenuationDb = 20.0 * Math.Log10(effectiveDistance);
        var sourceLevel = testModel.Source.Volume;
        var estimatedDirectLevelDb = sourceLevel - attenuationDb;

        var sourceArticleFactor = AcousticCalculationHelper.GetSourceArticleFactor(testModel.Source.Article);
        var directSignalFactor = AcousticCalculationHelper.GetDirectSignalFactor(estimatedDirectLevelDb);
        var rt60Factor = AcousticCalculationHelper.GetRt60Factor(rt60, roomRequirement.MinRt60, roomRequirement.MaxRt60);
        var perceivedClarity = AcousticCalculationHelper.GetPerceivedClarity(
            directSignalFactor,
            sourceArticleFactor,
            rt60Factor);
        var perceivedClarityLevel = AcousticCalculationHelper.GetClarityLevel(perceivedClarity);

        return new AnalysisResult
        {
            IsSuccess = true,
            Message = "Расчёт успешно выполнен.",
            TestModelId = testModel.Id,
            RoomName = room.Name,
            SourceName = testModel.Source.Name,
            ReceiverName = testModel.Receiver.Name,
            SourceLocation = testModel.SourceLocation,
            ReceiverLocation = testModel.ReceiverLocation,
            FormulaName = resolvedMethod.ToDisplayName(),
            Volume = volume,
            EquivalentAbsorptionArea = equivalentAbsorptionArea,
            AverageAbsorption = averageAbsorption,
            Rt60 = rt60,
            SourceReceiverDistance = distance,
            DistanceAttenuationDb = attenuationDb,
            EstimatedDirectLevelDb = estimatedDirectLevelDb,
            SourceArticleFactor = sourceArticleFactor,
            DirectSignalFactor = directSignalFactor,
            Rt60Factor = rt60Factor,
            PerceivedClarity = perceivedClarity,
            PerceivedClarityLevel = perceivedClarityLevel,
            Recommendation = BuildRecommendation(
                room.RoomType,
                roomRequirement.MinRt60,
                roomRequirement.MaxRt60,
                rt60,
                distance,
                estimatedDirectLevelDb,
                sourceArticleFactor,
                perceivedClarity,
                perceivedClarityLevel)
        };
    }

    private static AnalysisMethod ResolveAnalysisMethod(AnalysisMethod selectedMethod, double averageAbsorption)
    {
        if (selectedMethod != AnalysisMethod.Auto)
            return selectedMethod;

        return averageAbsorption >= 0.20
            ? AnalysisMethod.Eyring
            : AnalysisMethod.Sabine;
    }

    private static double CalculateSabine(double volume, double equivalentAbsorptionArea)
    {
        return 0.161 * volume / equivalentAbsorptionArea;
    }

    private static double CalculateEyring(double volume, double totalSurfaceArea, double averageAbsorption)
    {
        var safeAverageAbsorption = Math.Min(0.999, Math.Max(0.0001, averageAbsorption));
        return 0.161 * volume / (-totalSurfaceArea * Math.Log(1.0 - safeAverageAbsorption));
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

    private static AnalysisResult Fail(string message)
    {
        return new AnalysisResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    private static string BuildRecommendation(
        RoomType roomType,
        double minRt60,
        double maxRt60,
        double rt60,
        double distance,
        double directLevelDb,
        double sourceArticleFactor,
        double perceivedClarity,
        string perceivedClarityLevel)
    {
        var roomTypeName = roomType.ToDisplayName();

        string rtText;

        if (rt60 < minRt60)
        {
            rtText =
                $"Для типа помещения «{roomTypeName}» полученное время реверберации ниже рекомендуемого диапазона. Это может привести к избыточно сухому звучанию, хотя разборчивость речи, вероятно, останется высокой.";
        }
        else if (rt60 > maxRt60)
        {
            rtText =
                $"Для типа помещения «{roomTypeName}» полученное время реверберации превышает рекомендуемый диапазон. Это может ухудшать разборчивость речи и снижать акустический комфорт.";
        }
        else
        {
            rtText =
                $"Для типа помещения «{roomTypeName}» полученное время реверберации находится в рекомендуемом диапазоне.";
        }

        string distanceText = distance switch
        {
            < 1.0 => "Источник и приёмник расположены очень близко друг к другу, поэтому влияние расстояния на прямой сигнал минимально.",
            <= 3.0 => "Расстояние между источником и приёмником можно считать умеренным.",
            <= 5.0 => "Расстояние между источником и приёмником заметное, поэтому уровень прямого сигнала снижается.",
            _ => "Источник и приёмник расположены на значительном расстоянии, что приводит к выраженному ослаблению прямого сигнала."
        };

        string levelText = directLevelDb switch
        {
            < 45 => "Оценочный уровень прямого сигнала в точке приёмника является низким; рекомендуется уменьшить расстояние между источником и приёмником либо улучшить акустические свойства помещения.",
            < 55 => "Оценочный уровень прямого сигнала можно считать удовлетворительным, однако в неблагоприятных условиях он может оказаться недостаточным.",
            _ => "Оценочный уровень прямого сигнала в точке приёмника является достаточным."
        };

        string articleText = sourceArticleFactor switch
        {
            < 0.45 => "Артикуляция источника задана на низком уровне, что дополнительно снижает ожидаемую разборчивость речи.",
            < 0.70 => "Артикуляция источника находится на среднем уровне и умеренно влияет на итоговую оценку восприятия речи.",
            _ => "Артикуляция источника является достаточной и положительно влияет на восприятие речевого сигнала."
        };

        var clarityPercent = perceivedClarity * 100.0;
        var clarityText =
            $"Итоговая оценка воспринимаемой разборчивости — {perceivedClarityLevel} ({clarityPercent:F0}%).";

        return $"{rtText} {distanceText} {levelText} {articleText} {clarityText}";
    }
}
