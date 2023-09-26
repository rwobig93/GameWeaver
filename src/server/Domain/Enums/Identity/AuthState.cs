namespace Domain.Enums.Identity;

public enum AuthState
{
    Enabled = 0,
    Disabled = 1,
    LoginRequired = 2,
    LockedOut = 3,
    Unknown = 4
}