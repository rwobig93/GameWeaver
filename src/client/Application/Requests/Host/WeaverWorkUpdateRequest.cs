using Domain.Enums;

namespace Application.Requests.Host;

[MemoryPackable]
public partial class WeaverWorkUpdateRequest
{
    public int Id { get; set; }
    public HostWorkType Type { get; set; }
    public WeaverWorkState Status { get; set; }
    public object? WorkData { get; set; }
    public int AttemptCount { get; set; }
}