using Application.Models.GameServer.Developers;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.Publishers;

namespace Application.Models.GameServer.Game;

public class GameFull
{
    public Guid Id { get; set; }
    public string FriendlyName { get; set; } = "";
    public string SteamName { get; set; } = "";
    public int SteamGameId { get; set; }
    public int SteamToolId { get; set; }
    public GameProfileSlim DefaultGameProfile { get; set; } = null!;
    public string LatestBuildVersion { get; set; } = "";
    public string UrlBackground { get; set; } = "";
    public string UrlLogo { get; set; } = "";
    public string UrlLogoSmall { get; set; } = "";
    public string UrlSteamStorePage => $"https://store.steampowered.com/app/{SteamGameId}";
    public string UrlWebsite { get; set; } = "";
    public string ControllerSupport { get; set; } = "";
    public string DescriptionShort { get; set; } = "";
    public string DescriptionLong { get; set; } = "";
    public string DescriptionAbout { get; set; } = "";
    public string PriceInitial { get; set; } = "";
    public string PriceCurrent { get; set; } = "";
    public int PriceDiscount { get; set; }
    public int MetaCriticScore { get; set; }
    public string UrlMetaCriticPage { get; set; } = "";
    public string RequirementsPcMinimum { get; set; } = "";
    public string RequirementsPcRecommended { get; set; } = "";
    public string RequirementsMacMinimum { get; set; } = "";
    public string RequirementsMacRecommended { get; set; } = "";
    public string RequirementsLinuxMinimum { get; set; } = "";
    public string RequirementsLinuxRecommended { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool SupportsWindows { get; set; }
    public bool SupportsLinux { get; set; }
    public bool SupportsMac { get; set; }
    public List<GameGenreSlim> Genres { get; set; } = [];
    public List<PublisherSlim> Publishers { get; set; } = [];
    public List<DeveloperSlim> Developers { get; set; } = [];
}