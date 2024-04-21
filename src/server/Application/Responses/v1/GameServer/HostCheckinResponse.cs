using Application.Models.GameServer.WeaverWork;

namespace Application.Responses.v1.GameServer;

public class HostCheckInResponse
{
    public List<WeaverWorkClient> WorkList { get; set; } = [];
}