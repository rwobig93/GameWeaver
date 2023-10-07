using Domain.Enums.GameServer;

namespace Domain.DatabaseEntities.GameServer;

public class LocalResourceDb
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public bool Startup { get; set; } = false;
    public int StartupPriority { get; set; }
    public ResourceType Type { get; set; }
    public string Extension { get; set; } = "";
    public string Args { get; set; } = "";
}