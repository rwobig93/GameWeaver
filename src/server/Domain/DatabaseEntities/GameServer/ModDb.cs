namespace Domain.DatabaseEntities.GameServer;

public class ModDb
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public int SteamGameId { get; set; }
    public int SteamToolId { get; set; }
    public string SteamId { get; set; } = "";
    public string FriendlyName { get; set; } = "";
    public string CurrentHash { get; set; } = "";
}