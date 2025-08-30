using System.Collections.Specialized;
using Application.Auth.OauthClients;
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
using OAuth2.Configuration;
using OAuth2.Infrastructure;

namespace Infrastructure.Services.Integrations;

public class ExternalAuthProviderService : IExternalAuthProviderService
{
    private static string? _redirectUri;
    private static bool _anyProvidersEnabled;
    private static bool _enabledGoogle;
    private static bool _enabledDiscord;
    private static bool _enabledSpotify;
    private static bool _enabledCustomOne;
    private static bool _enabledCustomTwo;
    private static bool _enabledCustomThree;
    private static DiscordOauthClient? _discordClient;
    private static GoogleClient? _googleClient;
    private static SpotifyClient? _spotifyClient;
    private static GenericOauthClient? _customOneClient;
    private static GenericOauthClient? _customTwoClient;
    private static GenericOauthClient? _customThreeClient;
    private readonly ILogger _logger;
    private readonly OauthConfiguration _oauthConfig;

    public ExternalAuthProviderService(IOptions<OauthConfiguration> oauthConfig, IOptions<AppConfiguration> appConfig, ILogger logger)
    {
        _logger = logger;
        _oauthConfig = oauthConfig.Value;

        _redirectUri = new Uri(string.Concat(appConfig.Value.BaseUrl, AppRouteConstants.Identity.Login)).ToString();
        ConfigureDiscordClient();
        ConfigureGoogleClient();
        ConfigureSpotifyClient();
        ConfigureCustomOauthOneClient();
        ConfigureCustomOauthTwoClient();
        ConfigureCustomOauthThreeClient();
        UpdateProviderStatus();
    }

    public bool AnyProvidersEnabled => _anyProvidersEnabled;
    public bool ProviderEnabledGoogle => _enabledGoogle;
    public bool ProviderEnabledDiscord => _enabledDiscord;
    public bool ProviderEnabledSpotify => _enabledSpotify;
    public bool ProviderEnabledCustomOne => _enabledCustomOne;
    public bool ProviderEnabledCustomTwo => _enabledCustomTwo;
    public bool ProviderEnabledCustomThree => _enabledCustomThree;

    public async Task<IResult<string>> GetLoginUri(ExternalAuthProvider provider, ExternalAuthRedirect redirect)
    {
        string loginUri;

        switch (provider)
        {
            case ExternalAuthProvider.Discord:
                if (!ProviderEnabledDiscord)
                {
                    return await Result<string>.FailAsync($"{provider.ToString()} currently isn't enabled");
                }

                loginUri = await _discordClient!.GetLoginLinkUriAsync(ExternalAuthHelpers.GetAuthRedirectState(provider, redirect));
                break;
            case ExternalAuthProvider.Google:
                if (!ProviderEnabledGoogle)
                {
                    return await Result<string>.FailAsync($"{provider.ToString()} currently isn't enabled");
                }

                loginUri = await _googleClient!.GetLoginLinkUriAsync(ExternalAuthHelpers.GetAuthRedirectState(provider, redirect));
                break;
            case ExternalAuthProvider.Spotify:
                if (!ProviderEnabledSpotify)
                {
                    return await Result<string>.FailAsync($"{provider.ToString()} currently isn't enabled");
                }

                loginUri = await _spotifyClient!.GetLoginLinkUriAsync(ExternalAuthHelpers.GetAuthRedirectState(provider, redirect));
                break;
            case ExternalAuthProvider.CustomOne:
                if (!ProviderEnabledCustomOne)
                {
                    return await Result<string>.FailAsync($"{_oauthConfig.CustomOneProviderName} currently isn't enabled");
                }

                loginUri = await _customOneClient!.GetLoginLinkUriAsync(ExternalAuthHelpers.GetAuthRedirectState(provider, redirect));
                break;
            case ExternalAuthProvider.CustomTwo:
                if (!ProviderEnabledCustomTwo)
                {
                    return await Result<string>.FailAsync($"{_oauthConfig.CustomTwoProviderName} currently isn't enabled");
                }

                loginUri = await _customTwoClient!.GetLoginLinkUriAsync(ExternalAuthHelpers.GetAuthRedirectState(provider, redirect));
                break;
            case ExternalAuthProvider.CustomThree:
                if (!ProviderEnabledCustomThree)
                {
                    return await Result<string>.FailAsync($"{_oauthConfig.CustomThreeProviderName} currently isn't enabled");
                }

                loginUri = await _customThreeClient!.GetLoginLinkUriAsync(ExternalAuthHelpers.GetAuthRedirectState(provider, redirect));
                break;
            case ExternalAuthProvider.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
        }

        if (string.IsNullOrWhiteSpace(loginUri))
        {
            return await Result<string>.FailAsync($"Failed to get login uri from {provider.ToString()}");
        }

        return await Result<string>.SuccessAsync(loginUri);
    }

    public async Task<IResult<ExternalUserProfile>> GetUserProfile(ExternalAuthProvider provider, string oauthCode)
    {
        ExternalUserProfile externalProfile;

        try
        {
            switch (provider)
            {
                case ExternalAuthProvider.Discord:
                    if (!ProviderEnabledDiscord)
                    {
                        return await Result<ExternalUserProfile>.FailAsync($"{provider.ToString()} currently isn't enabled");
                    }

                    externalProfile =
                        (await _discordClient!.GetUserInfoAsync(new NameValueCollection {{"code", oauthCode}})).ToExternalProfile();
                    break;
                case ExternalAuthProvider.Google:
                    if (!ProviderEnabledGoogle)
                    {
                        return await Result<ExternalUserProfile>.FailAsync($"{provider.ToString()} currently isn't enabled");
                    }

                    externalProfile =
                        (await _googleClient!.GetUserInfoAsync(new NameValueCollection {{"code", oauthCode}})).ToExternalProfile();
                    break;
                case ExternalAuthProvider.Spotify:
                    if (!ProviderEnabledSpotify)
                    {
                        return await Result<ExternalUserProfile>.FailAsync($"{provider.ToString()} currently isn't enabled");
                    }

                    externalProfile =
                        (await _spotifyClient!.GetUserInfoAsync(new NameValueCollection {{"code", oauthCode}})).ToExternalProfile();
                    break;
                case ExternalAuthProvider.CustomOne:
                    if (!ProviderEnabledCustomOne)
                    {
                        return await Result<ExternalUserProfile>.FailAsync($"{_oauthConfig.CustomOneProviderName} currently isn't enabled");
                    }

                    externalProfile =
                        (await _customOneClient!.GetUserInfoAsync(new NameValueCollection {{"code", oauthCode}})).ToExternalProfile();
                    break;
                case ExternalAuthProvider.CustomTwo:
                    if (!ProviderEnabledCustomTwo)
                    {
                        return await Result<ExternalUserProfile>.FailAsync($"{_oauthConfig.CustomTwoProviderName} currently isn't enabled");
                    }

                    externalProfile =
                        (await _customTwoClient!.GetUserInfoAsync(new NameValueCollection {{"code", oauthCode}})).ToExternalProfile();
                    break;
                case ExternalAuthProvider.CustomThree:
                    if (!ProviderEnabledCustomThree)
                    {
                        return await Result<ExternalUserProfile>.FailAsync($"{_oauthConfig.CustomThreeProviderName} currently isn't enabled");
                    }

                    externalProfile =
                        (await _customThreeClient!.GetUserInfoAsync(new NameValueCollection {{"code", oauthCode}})).ToExternalProfile();
                    break;
                case ExternalAuthProvider.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }
        }
        catch (Exception ex)
        {
            return await Result<ExternalUserProfile>.FailAsync(ex.Message);
        }

        return await Result<ExternalUserProfile>.SuccessAsync(externalProfile);
    }

    private static void UpdateProviderStatus()
    {
        if (_enabledGoogle || _enabledDiscord || _enabledSpotify || _enabledCustomOne || _enabledCustomTwo || _enabledCustomThree)
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
            // SEE: https://discord.com/developers/docs/topics/oauth2
            _discordClient = new DiscordOauthClient(new RequestFactory(), new ClientConfiguration
            {
                ClientId = _oauthConfig.DiscordClientId.Trim(),
                ClientSecret = _oauthConfig.DiscordClientSecret.Trim(),
                RedirectUri = _redirectUri,
                Scope = "identify email guilds",
                IsEnabled = true,
                ClientTypeName = "WebClient"
            });
            _enabledDiscord = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to configure Discord Client for external Oauth");
            _enabledDiscord = false;
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
            _googleClient = new GoogleClient(new RequestFactory(), new ClientConfiguration
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
            _spotifyClient = new SpotifyClient(new RequestFactory(), new ClientConfiguration
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

    private void ConfigureCustomOauthOneClient()
    {
        if (string.IsNullOrWhiteSpace(_oauthConfig.CustomOneClientId) || string.IsNullOrWhiteSpace(_oauthConfig.CustomOneClientSecret))
        {
            _enabledCustomOne = false;
            return;
        }

        try
        {
            _customOneClient = new GenericOauthClient(new RequestFactory(), new ClientConfiguration
                {
                    ClientId = _oauthConfig.CustomOneClientId.Trim(),
                    ClientSecret = _oauthConfig.CustomOneClientSecret.Trim(),
                    RedirectUri = _redirectUri,
                    Scope = _oauthConfig.CustomOneScope,
                    IsEnabled = true,
                    ClientTypeName = "WebClient"
                }, _oauthConfig.CustomOneBaseUri, _oauthConfig.CustomOneUserInfoEndpoint, _oauthConfig.CustomOneAccessTokenEndpoint,
                _oauthConfig.CustomOneAccessCodeEndpoint, _oauthConfig.CustomOneProviderName);
            _enabledCustomOne = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to configure Custom Oauth One Client for external Oauth");
            _enabledCustomOne = false;
        }
    }

    private void ConfigureCustomOauthTwoClient()
    {
        if (string.IsNullOrWhiteSpace(_oauthConfig.CustomTwoClientId) || string.IsNullOrWhiteSpace(_oauthConfig.CustomTwoClientSecret))
        {
            _enabledCustomTwo = false;
            return;
        }

        try
        {
            _customTwoClient = new GenericOauthClient(new RequestFactory(), new ClientConfiguration
                {
                    ClientId = _oauthConfig.CustomTwoClientId.Trim(),
                    ClientSecret = _oauthConfig.CustomTwoClientSecret.Trim(),
                    RedirectUri = _redirectUri,
                    Scope = _oauthConfig.CustomTwoScope,
                    IsEnabled = true,
                    ClientTypeName = "WebClient"
                }, _oauthConfig.CustomTwoBaseUri, _oauthConfig.CustomTwoUserInfoEndpoint, _oauthConfig.CustomTwoAccessTokenEndpoint,
                _oauthConfig.CustomTwoAccessCodeEndpoint, _oauthConfig.CustomTwoProviderName);
            _enabledCustomTwo = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to configure Custom Oauth Two Client for external Oauth");
            _enabledCustomTwo = false;
        }
    }

    private void ConfigureCustomOauthThreeClient()
    {
        if (string.IsNullOrWhiteSpace(_oauthConfig.CustomThreeClientId) || string.IsNullOrWhiteSpace(_oauthConfig.CustomThreeClientSecret))
        {
            _enabledCustomThree = false;
            return;
        }

        try
        {
            _customThreeClient = new GenericOauthClient(new RequestFactory(), new ClientConfiguration
                {
                    ClientId = _oauthConfig.CustomThreeClientId.Trim(),
                    ClientSecret = _oauthConfig.CustomThreeClientSecret.Trim(),
                    RedirectUri = _redirectUri,
                    Scope = _oauthConfig.CustomThreeScope,
                    IsEnabled = true,
                    ClientTypeName = "WebClient"
                }, _oauthConfig.CustomThreeBaseUri, _oauthConfig.CustomThreeUserInfoEndpoint, _oauthConfig.CustomThreeAccessTokenEndpoint,
                _oauthConfig.CustomThreeAccessCodeEndpoint, _oauthConfig.CustomThreeProviderName);
            _enabledCustomThree = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to configure Custom Oauth Three Client for external Oauth");
            _enabledCustomThree = false;
        }
    }
}