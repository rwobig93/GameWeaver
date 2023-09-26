using System.ComponentModel.DataAnnotations;

namespace Application.Settings.AppSettings;

public class SecurityConfiguration : IAppSettingsSection
{
    public const string SectionName = "Security";

    [Required]
    [MinLength(32)]
    [MaxLength(128)]
    public string JsonTokenSecret { get; init; } = null!;
    
    [Required]
    [MinLength(32)]
    [MaxLength(128)]
    public string PasswordPepper { get; init; } = null!;
    
    [Range(0, 2_592_000)]
    public int PermissionValidationIntervalSeconds { get; init; } = 5;
    
    [Range(1, 44_000)]
    public int UserTokenExpirationMinutes { get; init; } = 15;
    
    [Range(1, 5_184_000)]
    public int ApiTokenExpirationMinutes { get; init; } = 60;
    
    [Range(16, 256)]
    public int UserApiTokenSizeInBytes { get; init; } = 128;
    
    [Range(0, 86_400)]
    public int SessionIdleTimeoutMinutes { get; init; } = 240;
    
    [Range(0, 86_400)]
    public int ForceLoginIntervalMinutes { get; init; } = 1440;
    
    [Range(1, 100)]
    public int MaxBadPasswordAttempts { get; init; } = 3;
    
    [Range(0, 86_400)]
    public int AccountLockoutMinutes { get; init; } = 15;
    
    public bool TrustAllCertificates { get; set; }
    
    public bool NewlyRegisteredAccountsDisabled { get; set; }
}