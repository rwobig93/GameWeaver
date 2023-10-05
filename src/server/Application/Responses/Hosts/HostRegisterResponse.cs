namespace Application.Responses.Hosts;

public class HostRegisterResponse
{
    public Guid HostId { get; set; }
    public string HostToken { get; set; } = null!;
}