using AcousticVision.Models;

namespace AcousticVision.Common;

public static class AcousticCalculationHelper
{
    public const double TextureInfluenceFactor = 0.3;

    public static double GetEffectiveAbsorption(RoomSurface surface)
    {
        var materialAlpha = surface.Material?.NoiseCancelation ?? 0.0;
        var textureAlpha = surface.Texture?.NoiseCancelation ?? 0.0;

        return GetEffectiveAbsorption(materialAlpha, textureAlpha);
    }

    public static double GetEffectiveAbsorption(double materialAlpha, double textureAlpha)
    {
        return Clamp01(materialAlpha + TextureInfluenceFactor * textureAlpha);
    }

    public static double GetSourceArticleFactor(double? article)
    {
        // Если артикуляция не задана, считаем источник нейтральным, чтобы старые данные не ухудшали результат.
        return article is null ? 1.0 : Clamp01(article.Value);
    }

    public static double GetDirectSignalFactor(double directLevelDb)
    {
        // Упрощённая нормализация оценочного уровня прямого сигнала к диапазону 0..1.
        // 35 дБ и ниже — слабый сигнал, 70 дБ и выше — высокий сигнал.
        return Clamp01((directLevelDb - 35.0) / 35.0);
    }

    public static double GetRt60Factor(double rt60, double minRt60, double maxRt60)
    {
        if (rt60 <= 0 || minRt60 <= 0 || maxRt60 <= minRt60)
            return 0.0;

        if (rt60 >= minRt60 && rt60 <= maxRt60)
            return 1.0;

        var range = maxRt60 - minRt60;
        var deviation = rt60 < minRt60
            ? minRt60 - rt60
            : rt60 - maxRt60;

        // Мягкий штраф за выход за рекомендуемый диапазон.
        return Clamp01(1.0 / (1.0 + deviation / range));
    }

    public static double GetPerceivedClarity(double directSignalFactor, double sourceArticleFactor, double rt60Factor)
    {
        return Clamp01(directSignalFactor * sourceArticleFactor * rt60Factor);
    }

    public static string GetClarityLevel(double perceivedClarity)
    {
        return perceivedClarity switch
        {
            >= 0.70 => "высокая",
            >= 0.45 => "средняя",
            >= 0.25 => "пониженная",
            _ => "низкая"
        };
    }

    public static double Clamp01(double value)
    {
        if (value < 0) return 0;
        if (value > 1) return 1;
        return value;
    }
}
