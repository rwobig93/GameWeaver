using Domain.Enums.Integration;

namespace Application.Requests.Identity.User;

public class UserExternalAuthLoginRequest
{
    public ExternalAuthProvider Provider { get; set; }
    public string Email { get; set; } = "";
    public string ExternalId { get; set; } = "";
}