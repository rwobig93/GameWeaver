namespace Application.Requests.v1.Identity.User;

public class UserRoleRequest
{
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public List<string> RoleNames { get; set; } = [];
}