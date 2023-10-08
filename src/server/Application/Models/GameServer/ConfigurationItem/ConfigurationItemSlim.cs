namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemSlim
{
    public Guid Id { get; set; }
    public Guid GameProfileId { get; set; }
    public string Path { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}