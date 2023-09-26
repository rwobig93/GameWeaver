namespace Application.Requests.Identity.User;

public class UserUpdatePasswordRequest
{
    public Guid Id { get; set; }
    public byte[] PasswordSalt { get; set; } = null!;
    public string PasswordHash { get; set; } = "";
}