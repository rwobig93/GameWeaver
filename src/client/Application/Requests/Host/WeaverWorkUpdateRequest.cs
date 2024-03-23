using Domain.Enums;

namespace Application.Requests.Host;

[MemoryPackable]
public partial class WeaverWorkUpdateRequest
{
    public int Id { get; set; }
    public WeaverWorkTarget Type { get; set; }
    public WeaverWorkState Status { get; set; }
    public byte[] WorkData { get; set; } = null!;
    public int AttemptCount { get; set; }
}