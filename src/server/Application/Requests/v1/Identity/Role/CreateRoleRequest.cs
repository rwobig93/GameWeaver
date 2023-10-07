namespace Application.Requests.v1.Identity.Role;

public class CreateRoleRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}