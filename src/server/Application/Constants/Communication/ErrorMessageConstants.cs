namespace Application.Constants.Communication;

public static class ErrorMessageConstants
{
    public static class Generic
    {
        public const string ContactAdmin = "An internal server error occurred, please contact the administrator";
        public const string InvalidValueError = "The value provided was invalid, please try again";
        public const string NotFound = "Was unable to find that resource, it doesn't exist";
    }

    public static class Users
    {
        public const string AccountDisabledError = "Your account is disabled, please contact the administrator";
        public const string AccountLockedOutError =
            "Your account is locked out due to bad password attempts, please contact the administrator or wait for the lockout expiration to end";
        public const string EmailNotConfirmedError = "Your email has not been confirmed, please confirm your email";
        public const string UserNotFoundError = "Was unable to find a user with the provided information";
        public const string ServiceAccountOnly = "This action is meant for service accounts. " + 
            "Please contact the administrator for a service account or generate your own token from the account page";
        public const string UserAccountOnly = "This action is meant for user accounts. Please login with a user account or register to get your own";
    }
    
    public static class Authentication
    {
        public const string CredentialsInvalidError = "The username and password combination provided is invalid";
        public const string PasswordsNoMatchError = "Provided password and confirmation don't match, please try again";
        public const string TokenInvalidError = "The token provided is invalid";
        public const string UnauthenticatedError = "You are currently unauthenticated and connot do that action, please login and try again";
        public const string ExternalAuthNotLinked =
            "This external account is not linked to your internal account, please login and link this account in your account settings";
    }

    public static class Permissions
    {
        public const string PermissionError = "You aren't authorized to do that, please go away";
        public const string CannotAdministrateMissingPermission =
            "You don't have the permission you are attempting to add/remove so you also can't administrate this permission";
    }

    public static class Roles
    {
        public const string CannotAdministrateAdminRole = "You aren't a valid role level to administrate this role";
        public const string AdminSelfPowerRemovalError =
            "You can't remove admin access from yourself, another admin will have to revoke your access";
        public const string DefaultAdminPowerRemovalError = "Default admin cannot have admin access revoked";
    }

    public static class Hosts
    {
        public const string MatchingRegistrationExists =
            "Active registration with matching description already exists, please use the existing registration or provide a different description";
        public const string RegisterNotFound =
            "Failure occurred attempting to register your host, please contact the administrator, this failure attempt has been logged";
    }
}