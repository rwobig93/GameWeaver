using System.Collections.Specialized;
using Application.Constants.Web;
using Application.Helpers.Integrations;
using Application.Mappers.Integrations;
using Application.Models.Identity.External;
using Application.Services.Integrations;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.Enums.Integrations;
using Microsoft.Extensions.Options;
using OAuth2.Client.Impl;
using OAuth2.Infrastructure;

namespace Infrastructure.Services.Integrations;

public class ExternalAuthProviderService : IExternalAuthProviderService
{
    public bool AnyProvidersEnabled => _anyProvidersEnabled;
    public bool ProviderEnabledGoogle => _enabledGoogle;
    public bool ProviderEnabledDiscord => _enabledDiscord;
    public bool ProviderEnabledSpotify => _enabledSpotify;

    private static string? _redirectUri;
    private static bool _anyProvidersEnabled;
    private static bool _enabledGoogle;
    private static bool _enabledDiscord;
    private static bool _enabledSpotify;
    private static GoogleClient? _googleClient;
    private static SpotifyClient? _spotifyClient;
    private readonly OauthConfiguration _oauthConfig;
    private readonly ILogger _logger;

    public ExternalAuthProviderService(IOptions<OauthConfiguration> oauthConfig, IOptions<AppConfiguration> appConfig, ILogger logger)
    {
        _logger = logger;
        _oauthConfig = oauthConfig.Value;

        _redirectUri = new Uri(string.Concat(appConfig.Value.BaseUrl, AppRouteConstants.Identity.Login)).ToString();
        ConfigureDiscordClient();
        ConfigureGoogleClient();
        ConfigureSpotifyClient();
        UpdateProviderStatus();
    }

    private static void UpdateProviderStatus()
    {
        if (_enabledGoogle || _enabledDiscord || _enabledSpotify)
        {
            _anyProvidersEnabled = true;
            return;
        }

        _anyProvidersEnabled = false;
    }

    private void ConfigureDiscordClient()
    {
        if (string.IsNullOrWhiteSpace(_oauthConfig.DiscordClientId) || string.IsNullOrWhiteSpace(_oauthConfig.DiscordClientSecret))
        {
            _enabledDiscord = false;
            return;
        }

        try
        {
            // Scopes: identify guilds
            _enabledDiscord = false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to configure Discord Client for external Oauth");
            _enabledGoogle = false;
        }
    }

    private void ConfigureGoogleClient()
    {
        if (string.IsNullOrWhiteSpace(_oauthConfig.GoogleClientId) || string.IsNullOrWhiteSpace(_oauthConfig.GoogleClientSecret))
        {
            _enabledGoogle = false;
            return;
        }

        try
        {
            _googleClient = new GoogleClient(new RequestFactory(), new OAuth2.Configuration.ClientConfiguration
            {
                ClientId = _oauthConfig.GoogleClientId.Trim(),
                ClientSecret = _oauthConfig.GoogleClientSecret.Trim(),
                RedirectUri = _redirectUri,
                Scope = "profile email",
                IsEnabled = true,
                ClientTypeName = "WebClient"
            });
            _enabledGoogle = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to configure Google Client for external Oauth");
            _enabledGoogle = false;
        }
    }

    private void ConfigureSpotifyClient()
    {
        if (string.IsNullOrWhiteSpace(_oauthConfig.SpotifyClientId) || string.IsNullOrWhiteSpace(_oauthConfig.SpotifyClientSecret))
        {
            _enabledSpotify = false;
            return;
        }

        try
        {
            _spotifyClient = new SpotifyClient(new RequestFactory(), new OAuth2.Configuration.ClientConfiguration
            {
                ClientId = _oauthConfig.SpotifyClientId.Trim(),
                ClientSecret = _oauthConfig.SpotifyClientSecret.Trim(),
                RedirectUri = _redirectUri,
                Scope = "user-read-email",
                IsEnabled = true,
                ClientTypeName = "WebClient"
            });
            _enabledSpotify = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to configure Spotify Client for external Oauth");
            _enabledSpotify = false;
        }
    }

    public async Task<IResult<string>> GetLoginUri(ExternalAuthProvider provider, ExternalAuthRedirect redirect)
    {
        string loginUri;

        switch (provider)
        {
            case ExternalAuthProvider.Discord:
                if (!ProviderEnabledDiscord)
                    return await Result<string>.FailAsync($"{provider.ToString()} currently isn't enabled");

                loginUri = "";
                break;
            case ExternalAuthProvider.Google:
                if (!ProviderEnabledGoogle)
                    return await Result<string>.FailAsync($"{provider.ToString()} currently isn't enabled");

                loginUri = await _googleClient!.GetLoginLinkUriAsync(ExternalAuthHelpers.GetAuthRedirectState(provider, redirect));
                break;
            case ExternalAuthProvider.Spotify:
                if (!ProviderEnabledSpotify)
                    return await Result<string>.FailAsync($"{provider.ToString()} currently isn't enabled");

                loginUri = await _spotifyClient!.GetLoginLinkUriAsync(ExternalAuthHelpers.GetAuthRedirectState(provider, redirect));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
        }

        if (string.IsNullOrWhiteSpace(loginUri))
            return await Result<string>.FailAsync($"Failed to get login uri from {provider.ToString()}");

        return await Result<string>.SuccessAsync(loginUri);
    }

    public async Task<IResult<ExternalUserProfile>> GetUserProfile(ExternalAuthProvider provider, string oauthCode)
    {
        ExternalUserProfile externalProfile;

        switch (provider)
        {
            case ExternalAuthProvider.Discord:
                if (!ProviderEnabledDiscord)
                    return await Result<ExternalUserProfile>.FailAsync($"{provider.ToString()} currently isn't enabled");

                externalProfile = new ExternalUserProfile();
                break;
            case ExternalAuthProvider.Google:
                if (!ProviderEnabledGoogle)
                    return await Result<ExternalUserProfile>.FailAsync($"{provider.ToString()} currently isn't enabled");

                externalProfile =
                    (await _googleClient!.GetUserInfoAsync(new NameValueCollection {{"code", oauthCode}})).ToExternalProfile();
                break;
            case ExternalAuthProvider.Spotify:
                if (!ProviderEnabledSpotify)
                    return await Result<ExternalUserProfile>.FailAsync($"{provider.ToString()} currently isn't enabled");

                externalProfile =
                    (await _spotifyClient!.GetUserInfoAsync(new NameValueCollection {{"code", oauthCode}})).ToExternalProfile();
                break;
            case ExternalAuthProvider.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
        }

        return await Result<ExternalUserProfile>.SuccessAsync(externalProfile);
    }
}