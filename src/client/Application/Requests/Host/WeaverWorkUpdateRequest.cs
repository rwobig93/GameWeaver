using Domain.Enums;

namespace Application.Requests.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class WeaverWorkUpdateRequest
{
    [MemoryPackOrder(0)]
    public int Id { get; set; }
    [MemoryPackOrder(1)]
    public WeaverWorkTarget Type { get; set; }
    [MemoryPackOrder(2)]
    public WeaverWorkState Status { get; set; }
    [MemoryPackOrder(3)]
    public byte[] WorkData { get; set; } = null!;
    [MemoryPackOrder(4)]
    public int AttemptCount { get; set; }
}