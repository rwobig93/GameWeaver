namespace Application.Requests.Identity.Role;

public class RoleMembershipRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}