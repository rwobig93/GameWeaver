using System.Security.Claims;
using System.Security.Principal;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.User;
using Application.Services.Lifecycle;
using Application.Settings.AppSettings;
using Domain.Enums.Identity;
using Domain.Models.Identity;
using Microsoft.Extensions.Options;
using GameWeaver.Settings;

namespace GameWeaver.Shared;

public partial class MainLayout
{
    [Inject] private IAppAccountService AccountService { get; set; } = null!;
    [Inject] private IRunningServerState ServerState { get; set; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IOptions<SecurityConfiguration> SecuritySettings { get; init; } = null!;
    [Inject] private IOptions<AppConfiguration> AppSettings { get; init; } = null!;
    

    public ClaimsPrincipal CurrentUser { get; set; } = new();
    
    public AppUserPreferenceFull _userPreferences = new();
    public readonly List<AppTheme> _availableThemes = AppThemes.GetAvailableThemes();
    public MudTheme _selectedTheme = AppThemes.DarkTheme.Theme;

    private AppUserFull UserFull { get; set; } = new();
    private bool _settingsDrawerOpen;
    private bool _canEditTheme;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ValidateAuthSession();
            await GetCurrentUser();
            await GetPermissions();
            await GetPreferences();
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
            // Failure occurred, user is unauthenticated or token has expired and will be handled by the permission auth handler
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canEditTheme = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Preferences.ChangeTheme);
    }

    private async Task ValidateAuthSession()
    {
        try
        {
            var loggedInUser = await CurrentUserService.GetCurrentUserPrincipal();
            
            // User is unauthenticated or authenticated so we'll just continue on loading the page
            if (loggedInUser == UserConstants.UnauthenticatedPrincipal ||
                loggedInUser != UserConstants.ExpiredPrincipal)
                return;
            
            // User is is expired so we'll attempt to re-authenticate the user
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
        catch (Exception)
        {
            await LogoutAndClearCache();
        }
    }

    private async Task LogoutAndClearCache()
    {
        // If we are already on the login page we'll just let the user get back to it
        var currentUri = new Uri(NavManager.Uri);
        if (currentUri.AbsolutePath == AppRouteConstants.Identity.Login)
            return;
        
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

    private static bool IsUserAuthenticated(IPrincipal? principal)
    {
        return principal?.Identity is not null && principal.Identity.IsAuthenticated;
    }

    private async Task RefreshPageData()
    {
        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task DrawerToggle()
    {
        _userPreferences.DrawerDefaultOpen = !_userPreferences.DrawerDefaultOpen;

        if (IsUserAuthenticated(CurrentUser))
            await AccountService.UpdatePreferences(CurrentUserService.GetIdFromPrincipal(CurrentUser), _userPreferences.ToUpdate());
    }

    private void SettingsToggle()
    {
        _settingsDrawerOpen = !_settingsDrawerOpen;
    }

    private async Task ChangeTheme(AppTheme theme)
    {
        if (!_canEditTheme) return;
        
        try
        {
            _userPreferences.ThemePreference = theme.Id;
            _selectedTheme = AppThemes.GetThemeById(theme.Id).Theme;
            
            if (IsUserAuthenticated(CurrentUser))
            {
                var userId = CurrentUserService.GetIdFromPrincipal(CurrentUser);
                var result = await AccountService.UpdatePreferences(userId, _userPreferences.ToUpdate());
                if (!result.Succeeded)
                    result.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            }
        }
        catch
        {
            _selectedTheme = AppThemes.GetThemeById(theme.Id).Theme;
        }
    }

    private async Task GetPreferences()
    {
        if (IsUserAuthenticated(CurrentUser))
        {
            var userId = CurrentUserService.GetIdFromPrincipal(CurrentUser);
            var preferences = await AccountService.GetPreferences(userId);
            if (!preferences.Succeeded)
            {
                preferences.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            _userPreferences = preferences.Data;
            UpdateCustomThemes();
            _selectedTheme = AppThemes.GetThemeById(_userPreferences.ThemePreference).Theme;
        }
    }

    private void UpdateCustomThemes()
    {
        foreach (var customThemeId in AppThemes.GetCustomThemeIds())
        {
            var matchingTheme = _availableThemes.FirstOrDefault(x => x.Id == customThemeId);
            var preferenceTheme = AppThemes.GetPreferenceCustomThemeFromId(_userPreferences, customThemeId);
            
            matchingTheme!.FriendlyName = preferenceTheme.ThemeName;
            matchingTheme.Description = preferenceTheme.ThemeDescription;
            matchingTheme.Theme.Palette = new PaletteDark()
            {
                Primary = preferenceTheme.ColorPrimary,
                Secondary = preferenceTheme.ColorSecondary,
                Tertiary = preferenceTheme.ColorTertiary,
                Background = preferenceTheme.ColorBackground,
                Success = preferenceTheme.ColorSuccess,
                Error = preferenceTheme.ColorError,
                BackgroundGrey = preferenceTheme.ColorNavBar,
                TextDisabled = "rgba(255,255,255, 0.26)",
                Surface = preferenceTheme.ColorBackground,
                DrawerBackground = preferenceTheme.ColorNavBar,
                DrawerText = preferenceTheme.ColorPrimary,
                AppbarBackground = preferenceTheme.ColorTitleBar,
                AppbarText = preferenceTheme.ColorPrimary,
                TextPrimary = preferenceTheme.ColorPrimary,
                TextSecondary = preferenceTheme.ColorSecondary,
                ActionDefault = "#adadb1",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                DrawerIcon = preferenceTheme.ColorPrimary
            };
        }
    }
}