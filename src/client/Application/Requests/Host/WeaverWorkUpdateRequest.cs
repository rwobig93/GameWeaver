using Domain.Enums;

namespace Application.Requests.Host;

public class WeaverWorkUpdateRequest
{
    public int Id { get; set; }
    public WeaverWorkState Status { get; set; }
    public string WorkData { get; set; } = "";
    public int AttemptCount { get; set; }
}