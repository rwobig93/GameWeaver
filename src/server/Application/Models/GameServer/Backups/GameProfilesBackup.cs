using Application.Models.GameServer.GameProfile;

namespace Application.Models.GameServer.Backups;

public class GameProfilesBackup : IBackupExport
{
    public string InstanceName { get; set; } = null!;
    public string InstanceVersion { get; set; } = null!;
    public DateTime ExportedTimestampUtc { get; set; } = DateTime.UtcNow;
    public List<GameProfileExport> GameProfiles { get; set; } = [];
}