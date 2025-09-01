namespace Application.Models.GameServer.Backups;

public interface IBackupExport
{
    public string InstanceName { get; set; }
    public string InstanceVersion { get; set; }
    public DateTime ExportedTimestampUtc { get; set; }
}