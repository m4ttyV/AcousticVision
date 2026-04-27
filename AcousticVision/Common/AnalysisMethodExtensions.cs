using AcousticVision.Models;

namespace AcousticVision.Common;

public static class AnalysisMethodExtensions
{
    public static string ToDisplayName(this AnalysisMethod method)
    {
        return method switch
        {
            AnalysisMethod.Auto => "Автоматический выбор",
            AnalysisMethod.Sabine => "Формула Сабина",
            AnalysisMethod.Eyring => "Формула Эйринга",
            _ => method.ToString()
        };
    }
}