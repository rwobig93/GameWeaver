namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemCreate
{
    public Guid ModifyingUserId { get; set; }
    public Guid LocalResourceId { get; set; }
    public bool DuplicateKey { get; set; }
    public string Path { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}