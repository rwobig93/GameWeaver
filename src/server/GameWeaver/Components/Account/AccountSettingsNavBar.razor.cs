using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Responses.v1.Identity;

namespace GameWeaver.Components.Account;

public partial class AccountSettingsNavBar
{
    [Inject] private IAppAccountService AccountService { get; set; } = null!;
    
    private UserBasicResponse CurrentUser { get; set; } = new();
    private bool _canEditTheme;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetCurrentUser();
            await GetPermissions();
            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        CurrentUser = await CurrentUserService.GetCurrentUserBasic() ?? new UserBasicResponse();
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canEditTheme = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Preferences.ChangeTheme);
    }

    private void NavigateToPage(string route)
    {
        NavManager.NavigateTo(route);
    }
}