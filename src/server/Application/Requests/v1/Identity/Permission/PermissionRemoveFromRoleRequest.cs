namespace Application.Requests.v1.Identity.Permission;

public class PermissionRemoveFromRoleRequest
{
    public Guid RoleId { get; set; } = Guid.Empty;
    public string PermissionValue { get; set; } = null!;
}