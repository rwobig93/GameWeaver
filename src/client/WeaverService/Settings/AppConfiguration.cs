using System.ComponentModel.DataAnnotations;
using Application.Settings;

namespace WeaverService.Settings;

public class AppConfiguration : IAppSettingsSection
{
    public const string SectionName = "General";
    
    [Url]
    public string ServerUrl { get; init; } = "https://localhost:9500/";
}