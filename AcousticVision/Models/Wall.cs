using System.Collections.Generic;

namespace AcousticVision.Models;

public class Wall
{
    public int Id { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    public int TextureId { get; set; }
    public Texture? Texture { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }

    public ICollection<RoomWall> RoomWalls { get; set; } = new List<RoomWall>();
}