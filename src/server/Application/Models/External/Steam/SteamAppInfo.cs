using Domain.Enums.GameServer;

namespace Application.Models.External.Steam;

public class SteamAppInfo
{
    public int AppId { get; set; }
    public string Name { get; set; } = "";
    public List<OsType> OsSupport { get; set; } = [];
    public string VersionBuild { get; set; } = "";
    public DateTime LastUpdatedUtc { get; set; }
}