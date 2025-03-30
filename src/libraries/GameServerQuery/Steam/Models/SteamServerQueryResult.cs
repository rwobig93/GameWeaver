namespace GameServerQuery.Steam.Models;

public class SteamServerQueryResult<TResult>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public TResult? Response { get; set; }
}