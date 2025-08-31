namespace Application.Settings.Models;

public class CustomOauthProviderSettings
{
    public string BaseUri { get; set; } = "";
    public string UserInfoEndpoint { get; set; } = "";
    public string AccessTokenEndpoint { get; set; } = "";
    public string AccessCodeEndpoint { get; set; } = "";
    public string ProviderName { get; set; } = "Custom SSO";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string Scope { get; set; } = "profile email";
    public string IdField { get; set; } = "id";
    public string NameField { get; set; } = "name";
    public string EmailField { get; set; } = "email";
    public string AvatarField { get; set; } = "avatar";
}