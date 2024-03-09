namespace Domain.Models.Host;

public class HostStorage
{
    public uint Index { get; set; }

    public string Model { get; set; } = null!;
    
    public string MountPoint { get; set; } = null!;

    public string Name { get; set; } = null!;

    public ulong TotalSpace { get; set; }

    public ulong FreeSpace { get; set; }
}