﻿namespace Application.Requests.GameServer.Game;

public class GameUpdateRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid? DefaultGameProfileId { get; set; }
    public string? LatestBuildVersion { get; set; }
    public string? UrlBackground { get; set; }
    public string? UrlLogo { get; set; }
    public string? UrlLogoSmall { get; set; }
    public string? UrlWebsite { get; set; }
    public string? ControllerSupport { get; set; }
    public string? DescriptionShort { get; set; }
    public string? DescriptionLong { get; set; }
    public string? DescriptionAbout { get; set; }
    public string? UrlMetaCriticPage { get; set; }
    public bool? SupportsWindows { get; set; }
    public bool? SupportsLinux { get; set; }
    public bool? SupportsMac { get; set; }
    public Guid? ManualFileRecordId { get; set; }
    public string? ManualVersionUrlCheck { get; set; }
    public string? ManualVersionUrlDownload { get; set; }
}