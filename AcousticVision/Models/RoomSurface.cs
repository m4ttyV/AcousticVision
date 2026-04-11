namespace AcousticVision.Models;

public class RoomSurface
{
    public int RoomId { get; set; }
    public RoomModel? Room { get; set; }

    public string Position { get; set; } = string.Empty;

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    public int TextureId { get; set; }
    public Texture? Texture { get; set; }
}
