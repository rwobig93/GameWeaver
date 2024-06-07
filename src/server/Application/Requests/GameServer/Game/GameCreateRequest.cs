namespace Application.Requests.GameServer.Game;

public class GameCreateRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = "";
    public int SteamGameId { get; set; }
    public int SteamToolId { get; set; }
    public bool SupportsWindows { get; set; }
    public bool SupportsLinux { get; set; }
    public bool SupportsMac { get; set; }
}