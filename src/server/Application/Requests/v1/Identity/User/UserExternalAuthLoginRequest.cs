using Domain.Enums.Integration;

namespace Application.Requests.v1.Identity.User;

public class UserExternalAuthLoginRequest
{
    public ExternalAuthProvider Provider { get; set; }
    public string Email { get; set; } = "";
    public string ExternalId { get; set; } = "";
}