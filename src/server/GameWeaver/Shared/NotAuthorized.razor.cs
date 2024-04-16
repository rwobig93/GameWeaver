using System.Security.Claims;
using Application.Constants.Identity;
using Application.Models.Identity.User;
using Application.Services.Lifecycle;
using Application.Settings.AppSettings;
using Blazored.LocalStorage;
using Domain.Enums.Identity;
using Infrastructure.Services.Auth;
using Microsoft.Extensions.Options;

namespace GameWeaver.Shared;

public partial class NotAuthorized
{
    [Inject] private IAppAccountService AccountService { get; set; } = null!;
    [Inject] private IRunningServerState ServerState { get; set; } = null!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IOptions<SecurityConfiguration> SecuritySettings { get; init; } = null!;
    [Inject] private IOptions<AppConfiguration> AppSettings { get; init; } = null!;
    [Inject] private AuthStateProvider AuthStateProvider { get; init; } = null!;
    
    
    public ClaimsPrincipal CurrentUser { get; set; } = new();
    private AppUserFull UserFull { get; set; } = new();
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ValidateAuthSession();
            await GetCurrentUser();
            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        try
        {
            CurrentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
            UserFull = (await CurrentUserService.GetCurrentUserFull())!;
        }
        catch (Exception)
        {
            // Failure occurred, start fresh
            await LogoutAndClearCache();
        }
    }

    private async Task ValidateAuthSession()
    {
        try
        {
            var loggedInUser = await CurrentUserService.GetCurrentUserPrincipal();
            
            // User is unauthenticated so they definitely don't have access, we'll let them see that for themselves
            if (loggedInUser == UserConstants.UnauthenticatedPrincipal)
                return;
            
            // User isn't unauthenticated and isn't expired then it's a valid authenticated user who doesn't have access to this resource
            if (loggedInUser != UserConstants.ExpiredPrincipal)
                return;
            
            // User is is expired so we'll attempt to re-authenticate the user since they could have access to this resource
            var tokenRequest = await AccountService.GetLocalStorage();
            if (!tokenRequest.Succeeded) return;

            if (string.IsNullOrWhiteSpace(tokenRequest.Data.ClientId) ||
                string.IsNullOrWhiteSpace(tokenRequest.Data.Token) ||
                string.IsNullOrWhiteSpace(tokenRequest.Data.RefreshToken))
            {
                // A token is missing so something happened, we'll just start over
                await LogoutAndClearCache();
                return;
            }

            var authState = await AuthStateProvider.GetAuthenticationStateAsync(tokenRequest.Data.Token);
            if (authState.User == UserConstants.ExpiredPrincipal)
            {
                // Session is expired so we'll attempt a re-authentication using the refresh token
                var refreshResponse = await AccountService.ReAuthUsingRefreshTokenAsync();
                if (!refreshResponse.Succeeded)
                {
                    // Using refresh token failed, user must do a fresh login
                    await LogoutAndClearCache();
                    return;
                }
                
                // Re-authentication was successful so we'll continue with an authenticated identity
                await AccountService.CacheTokensAndAuthAsync(refreshResponse.Data);
                NavManager.NavigateTo(NavManager.Uri, true);
            }
        }
        catch (Exception)
        {
            await LogoutAndClearCache();
        }
    }

    private async Task LogoutAndClearCache()
    {
        var loginRedirectReason = LoginRedirectReason.SessionExpired;

        try
        {
            // Validate if re-login is forced to give feedback to the user, items are ordered for overwrite precedence
            if (UserFull.Id != Guid.Empty)
            {
                var userSecurity = (await UserService.GetSecurityInfoAsync(UserFull.Id)).Data;
                
                // Force re-login was set on the account
                if (userSecurity!.AuthState == AuthState.LoginRequired)
                    loginRedirectReason = LoginRedirectReason.ReAuthenticationForce;
                // Last full login is older than the configured timeout
                if (userSecurity.LastFullLogin!.Value.AddMinutes(SecuritySettings.Value.ForceLoginIntervalMinutes) <
                    DateTimeService.NowDatabaseTime)
                    loginRedirectReason = LoginRedirectReason.FullLoginTimeout;
            }
        }
        catch
        {
            // Ignore any exceptions since we'll just be logging out anyway
        }

        await AccountService.LogoutGuiAsync(Guid.Empty);
        var loginUriBase = new Uri(string.Concat(AppSettings.Value.BaseUrl, AppRouteConstants.Identity.Login));
        var loginUriFull = QueryHelpers.AddQueryString(
            loginUriBase.ToString(), LoginRedirectConstants.RedirectParameter, loginRedirectReason.ToString());
        
        NavManager.NavigateTo(loginUriFull, true);
    }
}