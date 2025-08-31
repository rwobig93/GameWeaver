using Application.Helpers.Integrations;
using Application.Settings.Models;
using Newtonsoft.Json.Linq;
using OAuth2.Client;
using OAuth2.Configuration;
using OAuth2.Infrastructure;
using OAuth2.Models;
using RestSharp.Authenticators;

namespace Infrastructure.Auth.OauthClients;

public class GenericOauthClient : OAuth2Client
{
    public GenericOauthClient(IRequestFactory factory, IClientConfiguration configuration, string baseUri, string userInfoEndpoint, string accessTokenEndpoint,
        string accessCodeEndpoint, string providerName, string idField = "id", string nameField = "name", string emailField = "email", string avatarField = "avatar")
        : base(factory, configuration)
    {
        BaseUri = baseUri;
        UserInfoEndpoint = userInfoEndpoint;
        AccessTokenEndpoint = accessTokenEndpoint;
        AccessCodeEndpoint = accessCodeEndpoint;
        ProviderName = providerName;
        IdField = string.IsNullOrWhiteSpace(idField) ? "id" : idField;
        NameField = string.IsNullOrWhiteSpace(nameField) ? "name" : nameField;
        EmailField = string.IsNullOrWhiteSpace(emailField) ? "email" : emailField;
        AvatarField = string.IsNullOrWhiteSpace(avatarField) ? "avatar" : avatarField;
    }

    public GenericOauthClient(IRequestFactory factory, IClientConfiguration configuration, CustomOauthProviderSettings customOauthSettings)
        : this(factory, configuration, customOauthSettings.BaseUri, customOauthSettings.UserInfoEndpoint, customOauthSettings.AccessTokenEndpoint,
            customOauthSettings.AccessCodeEndpoint, customOauthSettings.ProviderName, customOauthSettings.IdField, customOauthSettings.NameField,
            customOauthSettings.EmailField, customOauthSettings.AvatarField)
    {
    }


    private string ProviderName { get; }
    private string BaseUri { get; }
    private string UserInfoEndpoint { get; }
    private string AccessCodeEndpoint { get; }
    private string AccessTokenEndpoint { get; }
    private string IdField { get; }
    private string NameField { get; }
    private string EmailField { get; }
    private string AvatarField { get; }

    public override string Name => ProviderName;

    protected override Endpoint AccessCodeServiceEndpoint => new() {BaseUri = BaseUri, Resource = AccessCodeEndpoint};

    protected override Endpoint AccessTokenServiceEndpoint => new() {BaseUri = BaseUri, Resource = AccessTokenEndpoint};

    protected override Endpoint UserInfoServiceEndpoint => new() {BaseUri = BaseUri, Resource = UserInfoEndpoint};

    protected override void BeforeGetUserInfo(BeforeAfterRequestArgs args)
    {
        args.Client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(AccessToken, "Bearer");
    }

    protected override UserInfo ParseUserInfo(string content)
    {
        var response = JObject.Parse(content);
        var root = response.SelectToken("user") ?? response.SelectToken("profile") ?? response;
        var userInfo = new UserInfo
        {
            FirstName = root.GetFirstMatchingToken([NameField, "display_name", "name", "first_name", "given_name", "nickname"]),
            Id = root.GetFirstMatchingToken([IdField, "id", "guid", "uuid", "gid", "sub"]),
            Email = root.GetFirstMatchingToken([EmailField, "email", "mail", "email_address"]),
            ProviderName = Name
        };

        userInfo.AvatarUri.Normal =
            userInfo.AvatarUri.Large =
                userInfo.AvatarUri.Small = root.GetFirstMatchingToken([AvatarField, "images[0].url", "avatar", "image"]);

        return userInfo;
    }
}