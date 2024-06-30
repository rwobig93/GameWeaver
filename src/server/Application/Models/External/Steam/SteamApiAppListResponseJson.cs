namespace Application.Models.External.Steam;

public class SteamApiAppListResponseJson
{
    public required SteamApiAppResponseJson[] Apps { get; set; }
}