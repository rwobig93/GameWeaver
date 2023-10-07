namespace Domain.Models;

public class Mod
{
    public int GameId { get; set; }
    public int ToolId { get; set; }
    public string SteamId { get; set; } = "";
    public string FriendlyName { get; set; } = "";
}