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

    [Range(100, 10000)]
    public int ControlServerWorkIntervalMs { get; set; } = 500;

    [Range(100, 10000)]
    public int HostWorkIntervalMs { get; set; } = 1000;

    [Range(100, 10000)]
    public int GameServerWorkIntervalMs { get; set; } = 1000;

    [Range(100, 10000)]
    public int ResourceGatherIntervalMs { get; set; } = 2000;

    [Range(10, 86_400)]
    public int GameServerStatusCheckIntervalSeconds { get; set; } = 30;

    public string AppDirectory { get; set; } = "./";

    [Range(1, 100)]
    public int SimultaneousQueueWorkCountMax { get; set; } = 5;

    [Range(1, 1000)]
    public int GameserverBackupsToKeep { get; set; } = 24;

    [Range(1, 44_640)]
    public int GameserverBackupIntervalMinutes { get; set; } = 60;
}