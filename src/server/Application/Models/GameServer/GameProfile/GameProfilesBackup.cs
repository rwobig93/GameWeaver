namespace Application.Models.GameServer.GameProfile;

public class GameProfilesBackup
{
    public string InstanceName { get; set; } = null!;
    public string InstanceVersion { get; set; } = null!;
    public DateTime ExportedTimestampUtc { get; set; } = DateTime.UtcNow;
    public List<GameProfileExport> GameProfiles { get; set; } = [];
}