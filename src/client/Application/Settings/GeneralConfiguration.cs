using System.ComponentModel.DataAnnotations;

namespace Application.Settings;

public class GeneralConfiguration : IAppSettingsSection
{
    public const string SectionName = "General";
    
    [Url]
    public string ServerUrl { get; init; } = "https://localhost:9500/";

    [Range(1, 100)]
    public int QueueMaxPerRun { get; set; } = 5;

    [Range(0, 20)]
    public int MaxQueueAttempts { get; set; } = 5;
}