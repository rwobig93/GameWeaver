using System.Security.Claims;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.User;
using Application.Services.Lifecycle;
using Domain.Models.Identity;
using GameWeaver.Settings;

namespace GameWeaver.Shared;

public partial class SettingsMenu
{
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;

    [Parameter] public AppUserPreferenceFull UserPreferences { get; set; } = new();
    [Parameter] public ClaimsPrincipal CurrentUser { get; set; } = new();
    [Parameter] public AppUserFull UserFull { get; set; } = new();
    [Parameter] public List<AppTheme> AvailableThemes { get; set; } = AppThemes.GetAvailableThemes();
    [Parameter] public MudTheme SelectedTheme { get; set; } = AppThemes.DarkTheme.Theme;
    [Parameter] public EventCallback<AppTheme> ThemeChanged { get; set; }

    private string _displayUsername = "";
    private string _cssThemedText = "";
    private string _styleThemedTextSelected = "color: var(--mud-palette-primary);";
    private string _styleThemedText = "color: var(--mud-palette-secondary);";
    private string _clientTimeZone = "GMT";
    private bool _canEditTheme;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetUser();
            await GetClientTimezone();
            GetAvailableThemes();
            UpdateDisplayUsername();
            UpdateThemedElements();
            StateHasChanged();
        }
    }

    private static bool IsUserAuthenticated(ClaimsPrincipal? principal)
    {
        return principal?.Identity is not null && principal.Identity.IsAuthenticated;
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canEditTheme = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Preferences.ChangeTheme);
    }

    private async Task GetUser()
    {
        UserFull = (await CurrentUserService.GetCurrentUserFull())!;
        UserPreferences = (await AccountService.GetPreferences(UserFull.Id)).Data;
    }

    private void GetAvailableThemes()
    {
        AvailableThemes = AppThemes.GetAvailableThemes();
    }

    private string GetCurrentThemeName()
    {
        var currentAppTheme = AvailableThemes.FirstOrDefault(x => x.Id == UserPreferences.ThemePreference)!;
        if (currentAppTheme.FriendlyName.Length <= 12)
            return currentAppTheme.FriendlyName;

        return currentAppTheme.FriendlyName[..12];
    }

    private void UpdateDisplayUsername()
    {
        if (CurrentUser.Identity?.Name is null)
        {
            _displayUsername = "";
            return;
        }

        if (CurrentUser.Identity.Name.Length <= 18)
        {
            _displayUsername = CurrentUser.Identity.Name;
            return;
        }

        _displayUsername = CurrentUser.Identity.Name[..18];
    }

    private async Task ChangeThemeOnLayout(AppTheme theme)
    {
        if (!_canEditTheme) return;

        await ThemeChanged.InvokeAsync(theme);
        GetAvailableThemes();
        StateHasChanged();
    }

    private async Task ToggleGamerMode(bool enabled)
    {
        if (!_canEditTheme) return;

        UserPreferences.GamerMode = enabled;
        var response = await AccountService.UpdatePreferences(UserFull.Id, UserPreferences.ToUpdate());
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        UpdateThemedElements();
        NavManager.Refresh();
        Snackbar.Add("Testing", Severity.Info);
    }

    private void UpdateThemedElements()
    {
        if (!UserPreferences.GamerMode)
        {
            _cssThemedText = "";
            _styleThemedTextSelected = "color: var(--mud-palette-primary);";
            return;
        }

        _cssThemedText = "rainbow-text";
        _styleThemedTextSelected = "";
    }

    private async Task LogoutUser()
    {
        await AccountService.LogoutGuiAsync(UserFull.Id);
        NavManager.NavigateTo(AppRouteConstants.Identity.Login, true);
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _clientTimeZone = clientTimezoneRequest.Data;
    }
}