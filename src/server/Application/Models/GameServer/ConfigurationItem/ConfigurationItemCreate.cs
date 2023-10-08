namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemCreate
{
    public Guid GameProfileId { get; set; }
    public string Path { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}