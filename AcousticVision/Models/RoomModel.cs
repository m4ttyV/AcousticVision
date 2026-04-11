using System.Collections.Generic;

namespace AcousticVision.Models;

public class RoomModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public ICollection<RoomSurface> RoomSurfaces { get; set; } = new List<RoomSurface>();
    public ICollection<TestModel> TestModels { get; set; } = new List<TestModel>();
}
