using Domain.Enums;

namespace Domain.Models;

public class LocationPointer
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool Startup { get; set; } = false;
    public LocationType Type { get; set; }
    public string Extension { get; set; }
    public string Args { get; set; }
    public List<ConfigurationSet> ConfigSets { get; set; }
}