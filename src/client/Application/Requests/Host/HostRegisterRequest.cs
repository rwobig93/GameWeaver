namespace Application.Requests.Host;

public class HostRegisterRequest
{
    public Guid HostId { get; set; }
    public string Key { get; set; } = null!;
}