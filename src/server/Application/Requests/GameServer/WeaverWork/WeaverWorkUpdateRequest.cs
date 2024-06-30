using Domain.Enums.GameServer;

namespace Application.Requests.GameServer.WeaverWork;

public class WeaverWorkUpdateRequest
{
    public int Id { get; set; }
    public WeaverWorkState? Status { get; set; }
    public object? WorkData { get; set; }
}