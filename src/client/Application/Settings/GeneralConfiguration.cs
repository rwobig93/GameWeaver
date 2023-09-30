using System.ComponentModel.DataAnnotations;

namespace Application.Settings;

public class GeneralConfiguration : IAppSettingsSection
{
    public const string SectionName = "General";
    
    [Url]
    public string ServerUrl { get; init; } = "https://localhost:9500/";
}