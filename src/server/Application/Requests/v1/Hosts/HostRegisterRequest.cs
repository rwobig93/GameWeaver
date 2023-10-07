namespace Application.Requests.v1.Hosts;

public class HostRegisterRequest
{
    public Guid HostId { get; set; }
    public string RegisterToken { get; set; } = null!;
}