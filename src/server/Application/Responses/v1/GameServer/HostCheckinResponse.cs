using Domain.Enums.GameServer;

namespace Application.Responses.v1.GameServer;

public class HostCheckInResponse
{
    // TODO: Finish translation from WeaverWork to HostCheckInResponse
    public int Id { get; set; }
    public Guid? GameServerId { get; set; }
    public WeaverWorkTarget TargetType { get; set; }
    public WeaverWorkState Status { get; set; }
    public string WorkData { get; set; } = "";
}