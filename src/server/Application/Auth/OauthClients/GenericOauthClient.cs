using Newtonsoft.Json.Linq;
using OAuth2.Client;
using OAuth2.Configuration;
using OAuth2.Infrastructure;
using OAuth2.Models;

namespace Application.Auth.OauthClients;

public class GenericOauthClient : OAuth2Client
{
    public GenericOauthClient(IRequestFactory factory, IClientConfiguration configuration, string baseUri, string userInfoEndpoint, string accessTokenEndpoint,
        string accessCodeEndpoint, string providerName)
        : base(factory, configuration)
    {
        BaseUri = baseUri;
        UserInfoEndpoint = userInfoEndpoint;
        AccessTokenEndpoint = accessTokenEndpoint;
        AccessCodeEndpoint = accessCodeEndpoint;
        ProviderName = providerName;
    }

    private string ProviderName { get; }
    private string BaseUri { get; }
    private string UserInfoEndpoint { get; }
    private string AccessCodeEndpoint { get; }
    private string AccessTokenEndpoint { get; }

    public override string Name => ProviderName;

    protected override Endpoint AccessCodeServiceEndpoint => new() {BaseUri = BaseUri, Resource = AccessCodeEndpoint};

    protected override Endpoint AccessTokenServiceEndpoint => new() {BaseUri = BaseUri, Resource = AccessTokenEndpoint};

    protected override Endpoint UserInfoServiceEndpoint => new() {BaseUri = BaseUri, Resource = UserInfoEndpoint};

    protected override UserInfo ParseUserInfo(string content)
    {
        var response = JObject.Parse(content);
        var userInfo = new UserInfo();
        userInfo.AvatarUri.Normal =
            userInfo.AvatarUri.Large =
                userInfo.AvatarUri.Small = response.SelectToken("images[0].url")?.ToString();

        userInfo.FirstName = response.SelectToken("display_name")?.ToString();
        userInfo.Id = response.SelectToken("id")?.ToString();
        userInfo.Email = response.SelectToken("email")?.ToString();
        userInfo.ProviderName = this.Name;
        return userInfo;
    }
}