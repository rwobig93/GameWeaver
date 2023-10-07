namespace Application.Models.GameServer.Game;

public class GameSlim
{
    public string FriendlyName { get; set; } = "";
    public string SteamName { get; set; } = "";
    public int SteamGameId { get; set; }
    public int SteamToolId { get; set; }
    public Guid DefaultGameProfileId { get; set; }
    public string UrlBackground { get; set; } = "";
    public string UrlLogo { get; set; } = "";
    public string UrlLogoSmall { get; set; } = "";
    public string UrlSteamStorePage => $"https://store.steampowered.com/app/{SteamGameId}";
    public string UrlWebsite { get; set; } = "";
    public string ControllerSupport { get; set; } = "";
    public string DescriptionShort { get; set; } = "";
    public string DescriptionLong { get; set; } = "";
    public string DescriptionAbout { get; set; } = "";
    public string Developers { get; set; } = "";
    public string Publishers { get; set; } = "";
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
}