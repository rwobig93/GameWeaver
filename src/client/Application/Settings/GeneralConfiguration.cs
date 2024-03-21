using System.ComponentModel.DataAnnotations;

namespace Application.Settings;

public class GeneralConfiguration : IAppSettingsSection
{
    public const string SectionName = "General";
    
    [Url]
    public string ServerUrl { get; init; } = "https://localhost:9500/";

    [Range(1, 100)]
    public int CommunicationQueueMaxPerRun { get; set; } = 5;

    [Range(0, 20)]
    public int MaxQueueAttempts { get; set; } = 5;

    public int ControlServerWorkIntervalMs { get; set; } = 1000;

    public int HostWorkIntervalMs { get; set; } = 1000;

    public int GameServerWorkIntervalMs { get; set; } = 1000;

    public int ResourceGatherIntervalMs { get; set; } = 2000;

    public string AppDirectory { get; set; } = "./";

    [Range(1, 100)]
    public int SimultaneousQueueWorkCountMax { get; set; } = 5;

    [Range(1, 10)]
    public int GameserverBackupsToKeep { get; set; } = 24;

    [Range(1, 10)]
    public int GameserverBackupIntervalMinutes { get; set; } = 60;
}