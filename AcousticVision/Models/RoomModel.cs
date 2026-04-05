using System.Collections.Generic;

namespace AcousticVision.Models;

public class RoomModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Эти поля пригодятся тебе для расчёта RT60
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public ICollection<RoomWall> RoomWalls { get; set; } = new List<RoomWall>();
    public ICollection<TestModel> TestModels { get; set; } = new List<TestModel>();
}