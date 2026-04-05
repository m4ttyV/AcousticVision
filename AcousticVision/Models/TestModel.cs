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
}