using Domain.Enums.GameServer;

namespace Application.Requests.GameServer.WeaverWork;

public class WeaverWorkCreateRequest
{
    public Guid HostId { get; set; }
    public WeaverWorkTarget TargetType { get; set; }
    public WeaverWorkState Status { get; set; } = WeaverWorkState.WaitingToBePickedUp;
    public object? WorkData { get; set; }
}