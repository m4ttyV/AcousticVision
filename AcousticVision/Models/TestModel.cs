using AcousticVision.Common;

namespace AcousticVision.Models;

public class TestModel
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public RoomModel? Room { get; set; }

    public int SourceId { get; set; }
    public SoundSource? Source { get; set; }

    public int ReceiverId { get; set; }
    public SoundReceiver? Receiver { get; set; }

    public string SourceLocation { get; set; } = string.Empty;
    public string ReceiverLocation { get; set; } = string.Empty;

    public AnalysisMethod AnalysisMethod { get; set; } = AnalysisMethod.Auto;

    public string AnalysisMethodDisplayName => AnalysisMethod.ToDisplayName();

    public string DisplayName
    {
        get
        {
            var roomName = Room?.Name ?? "Без помещения";
            var sourceName = Source?.Name ?? "Без источника";
            var receiverName = Receiver?.Name ?? "Без приёмника";

            return $"{roomName} / {sourceName} / {receiverName}";
        }
    }
}