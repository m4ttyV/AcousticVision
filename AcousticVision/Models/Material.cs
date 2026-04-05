using System.Collections.Generic;

namespace AcousticVision.Models;

public class Material
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double NoiseCancelation { get; set; }

    public ICollection<Wall> Walls { get; set; } = new List<Wall>();
}