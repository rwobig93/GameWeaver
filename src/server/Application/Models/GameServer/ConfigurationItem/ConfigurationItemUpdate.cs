namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemUpdate
{
    public Guid ModifyingUserId { get; set; }
    public Guid Id { get; set; }
    public Guid? GameProfileId { get; set; }
    public bool? DuplicateKey { get; set; }
    public string? Path { get; set; }
    public string? Category { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
}