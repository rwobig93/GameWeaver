namespace Application.Models.GameServer.Game;

public class GameCreate
{
    public string FriendlyName { get; set; } = "";
    public string SteamName { get; set; } = "";
    public int SteamGameId { get; set; }
    public int SteamToolId { get; set; }
    public Guid DefaultGameProfileId { get; set; }
    public string UrlBackground { get; set; } = "";
    public string UrlLogo { get; set; } = "";
    public string UrlLogoSmall { get; set; } = "";
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
}