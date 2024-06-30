namespace Application.Requests.Identity.Permission;

public class PermissionCreateForUserRequest
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Name { get; set; } = null!;
    public string Group { get; set; } = null!;
    public string Access { get; set; } = null!;
    public string Description { get; set; } = null!;
}