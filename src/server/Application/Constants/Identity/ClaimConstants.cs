namespace Application.Constants.Identity;

public static class ClaimConstants
{
    public const string Permission = "Permission";
    public const string AuthenticationType = "AuthenticationType";
    public const string DynamicPermission = "Dynamic";

    public static class AuthType
    {
        public const string User = "UserAuthentication";
        public const string Api = "ApiAuthentication";
        public const string Host = "HostAuthentication";
    }
}
