using System.Security.Claims;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
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

    private string _clientTimeZone = "GMT";
    private bool _canEditTheme;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetClientTimezone();
            StateHasChanged();
            await Task.CompletedTask;
        }
    }
    
    private static bool IsUserAuthenticated(ClaimsPrincipal? principal)
    {
        return principal?.Identity is not null && principal.Identity.IsAuthenticated;
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canEditTheme = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Preferences.ChangeTheme);
    }

    private string GetCurrentThemeName()
    {
        var currentAppTheme = AvailableThemes.FirstOrDefault(x => x.Id == UserPreferences.ThemePreference)!;
        if (currentAppTheme.FriendlyName.Length <= 12)
            return currentAppTheme.FriendlyName;
        
        return currentAppTheme.FriendlyName[..12];
    }

    private string GetDisplayUsername()
    {
        if (CurrentUser.Identity?.Name is null)
            return "";
        if (CurrentUser.Identity.Name.Length <= 18)
            return CurrentUser.Identity.Name;

        return CurrentUser.Identity.Name[..18];
    }

    private async Task ChangeThemeOnLayout(AppTheme theme)
    {
        if (!_canEditTheme) return;
        
        await ThemeChanged.InvokeAsync(theme);
        StateHasChanged();
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