namespace Application.Responses.v1.Identity;

public class UserFullResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public string AuthState { get; set; } = null!;
    public string AccountType { get; init; } = null!;
    public List<ExtendedAttributeResponse> ExtendedAttributes { get; set; } = [];
    public List<PermissionResponse> Permissions { get; set; } = [];
}