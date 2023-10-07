namespace Application.Requests.v1.Identity.User;

public class UserUpdatePasswordRequest
{
    public Guid Id { get; set; }
    public byte[] PasswordSalt { get; set; } = null!;
    public string PasswordHash { get; set; } = "";
}