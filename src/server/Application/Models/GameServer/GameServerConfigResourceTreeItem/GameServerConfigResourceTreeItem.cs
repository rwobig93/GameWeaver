namespace Application.Models.GameServer.GameServerConfigResourceTreeItem;

public class GameServerConfigResourceTreeItem
{
    public Guid Id { get; set; }
    public bool IsConfig { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool DuplicateKey { get; set; }
    public bool Ignore { get; set; }
    public string Path { get; set; } = "";
    public string Category { get; set; } = "";
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}