namespace Application.Requests.v1.Identity.User;

public class GetUserByEmailRequest
{
    public string Email { get; set; } = null!;
}