namespace Application.Models.External.Steam;

public class SteamApiAppListResponse
{
    public required SteamApiAppList AppList { get; set; }
}

public class SteamApiAppList
{
    public required SteamApiApp[] Apps { get; set; }
}

public class SteamApiApp
{
    public int AppId { get; set; }
    public string Name { get; set; } = "";
}
