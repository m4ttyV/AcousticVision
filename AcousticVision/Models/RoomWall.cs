namespace AcousticVision.Models;

public class RoomWall
{
    public int RoomId { get; set; }
    public RoomModel? Room { get; set; }

    public int WallId { get; set; }
    public Wall? Wall { get; set; }

    public string Position { get; set; } = string.Empty;
}