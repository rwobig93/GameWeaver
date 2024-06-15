namespace Domain.Enums.Identity;

public enum LoginRedirectReason
{
    SessionExpired = 0,
    ReAuthenticationForce = 1,
    Unknown = 2,
    FullLoginTimeout = 3,
    ReAuthenticationForceUser = 4
}