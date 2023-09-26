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
    public List<string> AlternativeUrls { get; set; } = new();
}