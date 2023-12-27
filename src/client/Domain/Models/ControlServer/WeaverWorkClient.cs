using Domain.Enums;

namespace Domain.Models.ControlServer;

public class WeaverWorkClient
{
    public int Id { get; set; }
    public Guid? GameServerId { get; set; }
    public WeaverWorkTarget TargetType { get; set; }
    public WeaverWorkState Status { get; set; }
    public string WorkData { get; set; } = "";
}