using System.Collections.Generic;

namespace AcousticVision.Models;

public class SoundReceiver
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Properties { get; set; }

    public ICollection<TestModel> TestModels { get; set; } = new List<TestModel>();
}