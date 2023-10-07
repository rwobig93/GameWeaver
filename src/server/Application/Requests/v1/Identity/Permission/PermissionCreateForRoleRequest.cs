namespace Application.Requests.v1.Identity.Permission;

public class PermissionCreateForRoleRequest
{
    public Guid RoleId { get; set; } = Guid.Empty;
    public string Name { get; set; } = null!;
    public string Group { get; set; } = null!;
    public string Access { get; set; } = null!;
    public string Description { get; set; } = null!;
}