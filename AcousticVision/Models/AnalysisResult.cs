namespace AcousticVision.Models;

public class AnalysisResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public int TestModelId { get; set; }

    public string RoomName { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;

    public string SourceLocation { get; set; } = string.Empty;
    public string ReceiverLocation { get; set; } = string.Empty;

    public string FormulaName { get; set; } = string.Empty;

    public double Volume { get; set; }
    public double EquivalentAbsorptionArea { get; set; }
    public double AverageAbsorption { get; set; }
    public double Rt60 { get; set; }

    public double SourceReceiverDistance { get; set; }
    public double DistanceAttenuationDb { get; set; }
    public double EstimatedDirectLevelDb { get; set; }

    public double SourceArticleFactor { get; set; }
    public double DirectSignalFactor { get; set; }
    public double Rt60Factor { get; set; }
    public double PerceivedClarity { get; set; }
    public string PerceivedClarityLevel { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;
}
