namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemSlim
{
    public Guid Id { get; set; }
    public Guid LocalResourceId { get; set; }
    public bool DuplicateKey { get; set; }
    public bool Ignore { get; set; }
    public string Path { get; set; } = "";
    public string Category { get; set; } = "";
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string FriendlyName { get; set; } = "";
}