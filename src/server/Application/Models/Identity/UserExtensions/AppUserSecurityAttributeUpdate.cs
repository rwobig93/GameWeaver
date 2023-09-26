using Domain.Enums.Identity;

namespace Application.Models.Identity.UserExtensions;

public class AppUserSecurityAttributeUpdate
{
    public Guid OwnerId { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public bool? TwoFactorEnabled { get; set; }
    public string? TwoFactorKey { get; set; }
    public AuthState? AuthState { get; set; }
    public DateTime? AuthStateTimestamp { get; set; }
    public int? BadPasswordAttempts { get; set; }
    public DateTime? LastBadPassword { get; set; }
    public DateTime? LastFullLogin { get; set; }
}