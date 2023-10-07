namespace Application.Requests.v1.Identity.User;

public class ChangeUserEnabledStateRequest
{
    public bool IsEnabled { get; set; }
    public Guid UserId { get; set; }
}