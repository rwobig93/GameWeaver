namespace Application.Requests.Identity.Permission;

public class PermissionRemoveFromUserRequest
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string PermissionValue { get; set; } = null!;
}