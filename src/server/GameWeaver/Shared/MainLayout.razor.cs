using System.Security.Claims;
using System.Security.Principal;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.User;
using Application.Services.Lifecycle;
using Application.Settings.AppSettings;
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
    private string _cssThemedText = "";

    private bool _settingsDrawerOpen;
    private bool _canEditTheme;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
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
        {
            await AccountService.UpdatePreferences(CurrentUserService.GetIdFromPrincipal(CurrentUser), _userPreferences.ToUpdate());
        }
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
                {
                    result.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                }
            }
        }
        catch
        {
            _selectedTheme = AppThemes.GetThemeById(theme.Id).Theme;
        }

        StateHasChanged();
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
            UpdateThemedElements();
            UpdateCustomThemes();
            _selectedTheme = AppThemes.GetThemeById(_userPreferences.ThemePreference).Theme;
            StateHasChanged();
        }
    }

    private void UpdateCustomThemes()
    {
        foreach (var customThemeId in AppThemes.GetCustomThemeIds())
        {
            var matchingTheme = _availableThemes.First(x => x.Id == customThemeId);
            var preferenceTheme = AppThemes.GetPreferenceCustomThemeFromId(_userPreferences, customThemeId);

            matchingTheme.FriendlyName = preferenceTheme.ThemeName;
            matchingTheme.Description = preferenceTheme.ThemeDescription;
            matchingTheme.Theme.PaletteDark = new PaletteDark
            {
                Primary = preferenceTheme.ColorPrimary,
                Secondary = preferenceTheme.ColorSecondary,
                Tertiary = preferenceTheme.ColorTertiary,
                Background = preferenceTheme.ColorBackground,
                Success = preferenceTheme.ColorSuccess,
                Info = preferenceTheme.ColorInfo,
                Error = preferenceTheme.ColorError,
                BackgroundGray = preferenceTheme.ColorNavBar,
                TextDisabled = "rgba(255,255,255, 0.26)",
                Surface = preferenceTheme.ColorSurface,
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

    private void UpdateThemedElements()
    {
        if (!_userPreferences.GamerMode)
        {
            _cssThemedText = "";
            return;
        }

        _cssThemedText = "rainbow-text";
    }
}