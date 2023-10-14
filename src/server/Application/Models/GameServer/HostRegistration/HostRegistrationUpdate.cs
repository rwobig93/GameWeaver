namespace Application.Models.GameServer.HostRegistration;

public class HostRegistrationUpdate
{
    public Guid Id { get; set; }
    public Guid? HostId { get; set; }
    public string? Description { get; set; }
    public bool? Active { get; set; }
    public string? Key { get; set; } = null!;
    public DateTime? ActivationDate { get; set; }
    public string? ActivationPublicIp { get; set; } = "";
    public Guid? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}