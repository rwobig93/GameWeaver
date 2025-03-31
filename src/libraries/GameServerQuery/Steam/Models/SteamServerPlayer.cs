namespace GameServerQuery.Steam.Models;

public class SteamServerPlayer
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public ulong Score { get; set; }
    public TimeSpan Duration { get; set; }
}