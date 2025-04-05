using System.ComponentModel.DataAnnotations;
using Application.Validators.DataAnnotations;

namespace Application.Settings.AppSettings;

public class AppConfiguration : IAppSettingsSection
{
    public const string SectionName = "General";

    public string ApplicationName { get; set; } = "GameWeaver";

    [Url]
    public string BaseUrl { get; init; } = "https://localhost:9500/";

    // ReSharper disable once CollectionNeverUpdated.Global
    [UrlList]
    public List<string> AlternativeUrls { get; set; } = [];

    [Range(100, 100_000)]
    public int ApiPaginatedMaxPageSize { get; set; } = 1000;

    public bool UseCurrency { get; set; } = true;

    public string CurrencyName { get; set; } = "Server Tokens";

    [Range(0, 999_999)]
    public int StartingCurrency { get; set; } = 3;

    public bool UpdateGamesFromSteam { get; set; } = true;

    public string SteamAppNameFilter { get; set; } = "dedicated server";

    public int HostOfflineAfterSeconds { get; set; } = 3;
}