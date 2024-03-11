using Domain.Enums;

namespace Application.Requests.Host;

[MemoryPackable]
public class WeaverWorkUpdateRequest
{
    public int Id { get; set; }
    public HostWorkType Type { get; set; }
    public WeaverWorkState Status { get; set; }
    public object WorkData { get; set; } = null!;
    public int AttemptCount { get; set; }
}