namespace Application.Requests.v1.Identity.User;

public class UserLoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}