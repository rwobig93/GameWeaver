namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemExport
{
    public bool DuplicateKey { get; set; }
    public bool Ignore { get; set; }
    public string Path { get; set; } = "";
    public string Category { get; set; } = "";
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string FriendlyName { get; set; } = "";
}