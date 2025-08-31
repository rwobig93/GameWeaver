using Newtonsoft.Json.Linq;
using OAuth2.Client;
using OAuth2.Configuration;
using OAuth2.Infrastructure;
using OAuth2.Models;
using RestSharp.Authenticators;

namespace Infrastructure.Auth.OauthClients;

public class DiscordOauthClient : OAuth2Client
{
    public DiscordOauthClient(IRequestFactory factory, IClientConfiguration configuration)
        : base(factory, configuration)
    {
    }

    public override string Name => "Discord";

    protected override Endpoint AccessCodeServiceEndpoint => new() {BaseUri = "https://discord.com", Resource = "/oauth2/authorize"};

    protected override Endpoint AccessTokenServiceEndpoint => new() {BaseUri = "https://discord.com/api", Resource = "/oauth2/token"};

    protected override Endpoint UserInfoServiceEndpoint => new() {BaseUri = "https://discord.com/api", Resource = "/oauth2/@me"};

    protected override void BeforeGetUserInfo(BeforeAfterRequestArgs args)
    {
        args.Client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(AccessToken, "Bearer");
    }

    protected override UserInfo ParseUserInfo(string content)
    {
        var response = JObject.Parse(content);
        var userInfo = new UserInfo {ProviderName = Name};

        var user = response.SelectToken("user");
        userInfo.Id = user?.SelectToken("id")?.ToString().Trim();
        userInfo.FirstName = user?.SelectToken("global_name")?.ToString().Trim();
        userInfo.Email = user?.SelectToken("email")?.ToString().Trim();

        userInfo.AvatarUri.Normal =
            userInfo.AvatarUri.Large =
                userInfo.AvatarUri.Small = $"https://cdn.discordapp.com/avatars/{userInfo.Id}/{user?.SelectToken("avatar")}.png?size=1024";

        return userInfo;
    }
}