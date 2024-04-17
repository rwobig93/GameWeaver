using Domain.Contracts;

namespace Application.Requests.Host;

public class HostAuthRequest : Result
{
    public Guid HostId { get; set; }
    public string HostToken { get; set; } = null!;
}