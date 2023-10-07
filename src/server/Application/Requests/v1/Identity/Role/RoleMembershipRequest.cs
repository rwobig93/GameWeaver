namespace Application.Requests.v1.Identity.Role;

public class RoleMembershipRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}