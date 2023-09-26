namespace Application.Models.Identity.External;

public class ExternalUserProfile
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? AvatarUri { get; set; }
}