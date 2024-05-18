using Domain.Enums.GameServer;

namespace Application.Models.Events;

public class WeaverWorkStatusEvent
{
    public int Id { get; set; }
    public Guid? HostId { get; set; }
    public WeaverWorkTarget TargetType { get; set; }
    public WeaverWorkState Status { get; set; }
}