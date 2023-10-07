namespace Application.Requests.v1.Identity.User;

public class UserRegisterRequest
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}