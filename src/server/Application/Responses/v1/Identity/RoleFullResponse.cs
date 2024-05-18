namespace Application.Responses.v1.Identity;

public class RoleFullResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<PermissionResponse> Permissions { get; set; } = [];
}
