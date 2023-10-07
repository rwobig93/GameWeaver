namespace Application.Requests.v1.Identity.Permission;

public class PermissionUpdateRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Group { get; set; }
    public string? Access { get; set; }
    public string? Description { get; set; }
}