namespace Application.Models.GameServer.Host;

public class HostRegistrationCreate
{
    public Guid HostId { get; set; }
    public string Description { get; set; } = null!;
    public bool Active { get; set; }
    public string Key { get; set; } = null!;
    public DateTime? ActivationDate { get; set; }
    public string ActivationPublicIp { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}