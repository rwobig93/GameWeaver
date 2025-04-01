using Domain.Enums.Identity;

namespace Application.Models.Identity.UserExtensions;

public class AppUserSecurityAttributeCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid OwnerId { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorKey { get; set; }
    public AuthState AuthState { get; set; }
    public DateTime? AuthStateTimestamp { get; set; }
    public int BadPasswordAttempts { get; set; } = 0;
    public DateTime? LastBadPassword { get; set; }
    public DateTime? LastFullLogin { get; set; }
}