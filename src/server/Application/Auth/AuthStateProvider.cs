using System.Net.Http.Headers;
using System.Security.Claims;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Auth;
using Application.Settings.AppSettings;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;

namespace Application.Auth;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger _logger;
    private readonly AppConfiguration _appConfig;
    private readonly SecurityConfiguration _securityConfig;

    public AuthStateProvider(HttpClient httpClient, IHttpContextAccessor contextAccessor, ILocalStorageService localStorage, ILogger logger,
        IOptions<AppConfiguration> appConfig, IOptions<SecurityConfiguration> securityConfig)
    {
        _contextAccessor = contextAccessor;
        _localStorage = localStorage;
        _logger = logger;
        _securityConfig = securityConfig.Value;
        _appConfig = appConfig.Value;
        _httpClient = httpClient;
    }

    private string _authToken = "";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            await GetAuthTokenFromSession();

            var currentPrincipal = JwtHelpers.GetClaimsPrincipalFromToken(_authToken, _securityConfig, _appConfig);
            if (currentPrincipal is null || currentPrincipal == UserConstants.UnauthenticatedPrincipal || string.IsNullOrWhiteSpace(_authToken))
                return new AuthenticationState(UserConstants.UnauthenticatedPrincipal);

            if (currentPrincipal == UserConstants.ExpiredPrincipal)
                return new AuthenticationState(UserConstants.ExpiredPrincipalId(JwtHelpers.GetJwtUserId(_authToken)));

            // User is valid and not token isn't expired
            return GenerateNewAuthenticationState(_authToken);
        }
        catch
        {
            return new AuthenticationState(UserConstants.UnauthenticatedPrincipal);
        }
    }

    public async Task<AuthenticationState> GetAuthenticationStateAsync(string providedToken)
    {
        _authToken = providedToken;
        return await GetAuthenticationStateAsync();
    }

    private AuthenticationState GenerateNewAuthenticationState(string savedToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);

        var authorizedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(JwtHelpers.GetJwtDecoded(savedToken).Claims, JwtBearerDefaults.AuthenticationScheme));
        _contextAccessor.HttpContext!.User = authorizedPrincipal;

        var state = new AuthenticationState(authorizedPrincipal);
        AuthenticationStateUser = state.User;
        return state;
    }

    private async Task GetAuthTokenFromSession()
    {
        if (!string.IsNullOrWhiteSpace(_authToken)) return;

        _authToken = GetTokenFromHttpSession();
        if (!string.IsNullOrWhiteSpace(_authToken)) return;

        _authToken = await GetTokenFromLocalStorage();
        if (!string.IsNullOrWhiteSpace(_authToken)) return;

        _authToken = _httpClient.DefaultRequestHeaders.Authorization?.ToString() ?? "";
    }

    private string GetTokenFromHttpSession()
    {
        try
        {
            var headerHasValue = _contextAccessor.HttpContext!.Request.Headers.TryGetValue("Authorization", out var bearer);
            return !headerHasValue ? "" :
                // Authorization header should always be: <scheme> <token>, which in our case is: Bearer JWT
                bearer.ToString().Split(' ')[1];
        }
        catch
        {
            return "";
        }
    }

    private async Task<string> GetTokenFromLocalStorage()
    {
        try
        {
            return await _localStorage.GetItemAsync<string>(LocalStorageConstants.AuthToken) ?? string.Empty;
        }
        catch
        {
            // Since Blazor Server pre-rendering has the state received twice, and we can't have JSInterop run while rendering is occurring
            //   we have to do this to keep our sanity, would love to find a working solution to this at some point
            return "";
        }
    }

    public ClaimsPrincipal AuthenticationStateUser { get; private set; } = null!;

    public void DeAuthenticateUser()
    {
        try
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            Task.FromResult(new AuthenticationState(anonymousUser));
        }
        catch (Exception ex)
        {
            _logger.Warning("Error occurred attempting to de-authenticate user: {ErrorMessage}", ex.Message);
        }

        // NotifyAuthenticationStateChanged(authState);
    }

    public async Task<ClaimsPrincipal> GetAuthenticationStateProviderUserAsync()
    {
        var state = await GetAuthenticationStateAsync();
        return state.User;
    }
}