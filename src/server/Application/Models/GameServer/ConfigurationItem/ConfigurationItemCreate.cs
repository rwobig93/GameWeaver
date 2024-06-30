namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemCreate
{
    public Guid LocalResourceId { get; set; }
    public bool DuplicateKey { get; set; }
    public string Path { get; set; } = "";
    public string Category { get; set; } = "";
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string FriendlyName { get; set; } = "";
}