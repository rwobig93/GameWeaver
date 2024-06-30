namespace Application.Constants.Identity;

public static class LoginRedirectConstants
{
    public const string RedirectParameter = "redirectReason";
    public const string OauthCode = "code";
    public const string OauthState = "state";
    
    public const string SessionExpired = "Your session has expired and you need to re-login";
    public const string ReAuthenticationForce = "Your session was forcefully logged out by an Administrator and you are required to re-login";
    public const string ReAuthenticationForceUser = "Your session was forcefully logged out by your account and you are required to re-login";
    public const string Unknown = "An error occurred with your session, please re-login";
    public const string FullLoginTimeout = "Your last full login has passed the configured timeout, please re-login";
    public const string LockedOut = "Your account is currently locked out, you can wait to be unlocked, contact and administrator or reset your password";
    public const string Disabled = "Your account is currently disabled, please reach out to an administrator for details";
}