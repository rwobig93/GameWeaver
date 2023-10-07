namespace Application.Requests.v1.Identity.User;

public class GetUserByUsernameRequest
{
    public string Username { get; set; } = null!;
}