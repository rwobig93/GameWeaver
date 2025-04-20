using Application.Constants.GameServer;

namespace Application.Constants.Communication;

public static class ErrorMessageConstants
{
    public static class Generic
    {
        public const string ContactAdmin = "An internal server error occurred, please contact the administrator";
        public const string InvalidValueError = "The value provided was invalid, please try again";
        public const string NotFound = "Was unable to find that resource, it doesn't exist";
        public const string JsonInvalid = "The provided data is not a valid JSON serializable format, please verify your request body";
    }

    public static class Troubleshooting
    {
        public static string RecordId(Guid recordId) => $"Please mention this record Id: {recordId}";
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
        public const string TokenExpiredError = "Your authentication token has expired, please remove your authorization header and login again";
        public const string TokenMalformedError =
            "Your token is malformed, please remove the token and grab a new one. If you are trying to get a token please remove any existing token from your headers";
        public const string UnauthenticatedError = "You are currently unauthenticated and connot do that action, please login and try again";
        public const string ExternalAuthNotLinked =
            "This external account is not linked to your internal account, please login and link this account in your account settings";
    }

    public static class Permissions
    {
        public const string NotFound = "Was unable to find a permission using the information provided";
        public const string PermissionError = "You aren't authorized to do that, please go away";
        public const string CannotAdministrateMissingPermission =
            "You don't have the permission you are attempting to add/remove so you also can't administrate this permission";
        public const string Forbidden = "You hath been forbidden, do thy bidding my masta, it's a disasta, skywalka we're afta!";
        public const string NoViewPermission = "You don't have permission to view this resource";
        public const string DynamicPermissionNotSupported = "Permissions aren't supported for this entity type, please let an admin know";
    }

    public static class Roles
    {
        public const string NotFound = "Was unable to find a role using the information provided";
        public const string CannotAdministrateAdminRole = "You aren't a valid role level to administrate this role";
        public const string AdminSelfPowerRemovalError =
            "You can't remove admin access from yourself, another admin will have to revoke your access";
        public const string DefaultAdminPowerRemovalError = "Default admin cannot have admin access revoked";
        public const string RoleUsersAreStatic = "This role cannot have members added or removed manually";
    }

    public static class Hosts
    {
        public const string MatchingRegistrationExists =
            "Active registration with matching description already exists, please use the existing registration or provide a different description";
        public const string RegistrationNotFound = "The registration you provided is invalid, this failure attempt has been logged";
        public const string AssignedGameServers = "Game servers are assigned to this host, unable to delete the host while there are assignments";
        public const string NotFound = "Couldn't find a host matching the information provided, please verify the information provided";
    }

    public static class GameProfiles
    {
        public const string NotFound = "Was unable to find a game profile using the information provided";
        public const string MatchingName = "The profile name you provided already matches an existing profile, please provide a different name";
        public const string EmptyName = "The profile name you provided is empty, profiles MUST have a unique name";
        public const string DefaultProfileNotFound = "The selected game is currently missing a default profile, please create a profile for the game first";
        public const string ParentProfileNotFound = "The provided parent profile Id doesn't exist, please provide a valid parent profile Id";
        public const string DeleteDefaultProfile = "Game profile is the default for it's game, unable to delete the profile without deleting the game first";
        public const string AssignedGameServers = "Game profile is assigned to game servers, unable to delete the profile without removing the assignments";
        public const string NoStartupResources =
            "Game profile currently doesn't have any startup local resources, at least one startup resource must be available to start a game server";
        public const string InvalidNamePrefix = $"Profile names cannot start with one of the reserved prefixes: '{GameProfileConstants.ServerProfileNamePrefix}' or" +
                                          $" '{GameProfileConstants.GameProfileDefaultNamePrefix}'";
    }

    public static class Games
    {
        public const string NotFound = "Was unable to find a game using the information provided, please verify the information provided";
        public const string AssignedGameServers = "Game is assigned to game servers, unable to delete the game without deleting the game servers";
        public const string DuplicateSteamToolId = "A game with that Steam Tool Id already exists";
        public const string InvalidSteamToolId = "The Steam Tool Id provided is invalid, please verify your input";
        public const string NotManualGame = "The chosen game is not a manual sourced game so we can't do that";
        public const string NoServerClient = "The desired game doesn't have a server client uploaded yet, please upload one or have an administrator do so";
    }

    public static class Developers
    {
        public const string NotFound = "Was unable to find a developer using the information provided, please verify the information provided";
    }

    public static class Publishers
    {
        public const string NotFound = "Was unable to find a publisher using the information provided, please verify the information provided";
    }

    public static class GameGenres
    {
        public const string NotFound = "Was unable to find a game genre using the information provided, please verify the information provided";
    }

    public static class GameServers
    {
        public const string NotFound = "Was unable to find a game server using the information provided";
        public const string DefaultProfileAssignment = "The default profile for a game cannot be assigned to a game server";

        public static string InsufficientCurrency(string currencyName) => $"You don't have enough {currencyName} to create a game server";
    }

    public static class LocalResources
    {
        public const string NotFound = "Was unable to find a local resource using the information provided, please verify the information provided";
        public const string DuplicateResource = "The provided resource information matches an already existing resource, please verify the information provided";
    }

    public static class ConfigItems
    {
        public const string NotFound = "Was unable to find a configuration item using the information provided, please verify the information provided";
        public const string DuplicateConfig = "The provided config information matches an already existing config, please verify the information provided";
    }

    public static class Mods
    {
        public const string NotFound = "Was unable to find a mod using the information provided, please verify the information provided";
    }

    public static class WeaverWork
    {
        public const string NotFound = "Was unable to find weaver work using the information provided, please verify the information provided";
        public const string InvalidWorkData = "Work data is null, please verify your input";
        public const string InvalidDeserializedWorkData = "Deserialized work data is null, please verify your input";
    }

    public static class FileStorage
    {
        public const string NotFound = "Was unable to find a file matching the information provided, please verify your input";
    }
}