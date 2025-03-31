namespace GameServerQuery.Steam.Models;

public class SteamServerPlayersInfo
{
    public int PlayerCount { get; set; }
    public List<SteamServerPlayer> Players { get; set; } = [];
    public ulong TotalScore { get; set; }
    public ulong TheShipDeaths { get; set; }
    public ulong TheShipMoney { get; set; }
}