namespace Application.Settings.AppSettings;

public class OauthConfiguration : IAppSettingsSection
{
    public const string SectionName = "Oauth";

    public string DiscordClientId { get; set; } = "";
    public string DiscordClientSecret { get; set; } = "";

    public string GoogleClientId { get; set; } = "";
    public string GoogleClientSecret { get; set; } = "";

    public string SpotifyClientId { get; set; } = "";
    public string SpotifyClientSecret { get; set; } = "";

    public string CustomOneBaseUri { get; set; } = "";
    public string CustomOneUserInfoEndpoint { get; set; } = "";
    public string CustomOneAccessTokenEndpoint { get; set; } = "";
    public string CustomOneAccessCodeEndpoint { get; set; } = "";
    public string CustomOneProviderName { get; set; } = "Custom SSO One";
    public string CustomOneClientId { get; set; } = "";
    public string CustomOneClientSecret { get; set; } = "";
    public string CustomOneScope { get; set; } = "profile email";

    public string CustomTwoBaseUri { get; set; } = "";
    public string CustomTwoUserInfoEndpoint { get; set; } = "";
    public string CustomTwoAccessTokenEndpoint { get; set; } = "";
    public string CustomTwoAccessCodeEndpoint { get; set; } = "";
    public string CustomTwoProviderName { get; set; } = "Custom SSO Two";
    public string CustomTwoClientId { get; set; } = "";
    public string CustomTwoClientSecret { get; set; } = "";
    public string CustomTwoScope { get; set; } = "profile email";

    public string CustomThreeBaseUri { get; set; } = "";
    public string CustomThreeUserInfoEndpoint { get; set; } = "";
    public string CustomThreeAccessTokenEndpoint { get; set; } = "";
    public string CustomThreeAccessCodeEndpoint { get; set; } = "";
    public string CustomThreeProviderName { get; set; } = "Custom SSO Three";
    public string CustomThreeClientId { get; set; } = "";
    public string CustomThreeClientSecret { get; set; } = "";
    public string CustomThreeScope { get; set; } = "profile email";
}