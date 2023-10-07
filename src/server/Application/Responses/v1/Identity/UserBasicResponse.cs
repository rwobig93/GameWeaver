namespace Application.Responses.v1.Identity;

public class UserBasicResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public string AuthState { get; set; } = null!;
    public string AccountType { get; init; } = null!;
}