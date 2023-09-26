namespace Application.Requests.Identity.User;

public class UserLoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}