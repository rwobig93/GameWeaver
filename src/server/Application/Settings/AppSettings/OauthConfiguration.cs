using Application.Settings.Models;

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

    public CustomOauthProviderSettings CustomProviderOne { get; set; } = new() {ProviderName = "Custom SSO Primary"};
    public CustomOauthProviderSettings CustomProviderTwo { get; set; } = new() {ProviderName = "Custom SSO Secondary"};
    public CustomOauthProviderSettings CustomProviderThree { get; set; } = new() {ProviderName = "Custom SSO Tertiary"};
}