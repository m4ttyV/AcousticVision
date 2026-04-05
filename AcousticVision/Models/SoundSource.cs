using System.Collections.Generic;

namespace AcousticVision.Models;

public class SoundSource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Пока храним как строку, чтобы не усложнять
    public string Location { get; set; } = string.Empty;

    public double Volume { get; set; }
    public double? Article { get; set; }
    public string? Properties { get; set; }

    public ICollection<TestModel> TestModels { get; set; } = new List<TestModel>();
}