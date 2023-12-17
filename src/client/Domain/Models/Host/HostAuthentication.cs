namespace Domain.Models.Host;

public class HostAuthentication
{
    public Guid HostId { get; set; }
    public string HostToken { get; set; } = null!;
}