namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemUpdate
{
    public Guid Id { get; set; }
    public Guid? GameProfileId { get; set; }
    public string? Path { get; set; }
    public string? Category { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
}