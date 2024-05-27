namespace Domain.DatabaseEntities.GameServer;

public class ConfigurationItemDb
{
    public Guid Id { get; set; }
    public Guid LocalResourceId { get; set; }
    public bool DuplicateKey { get; set; }
    public string Path { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string FriendlyName { get; set; } = "";
}