using Domain.Enums.Integrations;
using MemoryPack;

namespace Application.Responses.v1.GameServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class GameDownloadResponse
{
    [MemoryPackOrder(0)]
    public Guid Id { get; set; }
    [MemoryPackOrder(1)]
    public FileStorageFormat Format { get; set; }
    [MemoryPackOrder(2)]
    public string GameName { get; set; } = "";
    [MemoryPackOrder(3)]
    public string FileName { get; set; } = null!;
    [MemoryPackOrder(4)]
    public string Version { get; set; } = "";
    [MemoryPackOrder(5)]
    public string HashSha256 { get; set; } = null!;
    [MemoryPackOrder(6)]
    public byte[] Content { get; set; } = null!;
}