using Application.Models.GameServer.Game;

namespace Application.Models.GameServer.Backups;

public class GamesBackup : IBackupExport
{
    public string InstanceName { get; set; } = null!;
    public string InstanceVersion { get; set; } = null!;
    public DateTime ExportedTimestampUtc { get; set; } = DateTime.UtcNow;
    public List<GameExport> Games { get; set; } = [];
}