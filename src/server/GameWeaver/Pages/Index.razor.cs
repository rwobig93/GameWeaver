using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.User;
using Application.Services.Lifecycle;

namespace GameWeaver.Pages;

public partial class Index
{
    // MainLayout has a CascadingParameter of itself, this allows the refresh button on the AppBar to refresh all page state data
    //  If this parameter isn't cascaded to a page then the refresh button won't affect that pages' state data
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;

    private AppUserFull _loggedInUser = new();
    
    private bool _canViewApi;
    private bool _canViewJobs;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await UpdateLoggedInUser();
                await GetPermissions();
                StateHasChanged();
            }
        }
        catch
        {
            // TODO: Not getting redirect error after fixing auth handling (need to re-login or idle too long, ect), look at removing per page handling
            // User has old saved token, so we'll force a local storage clear and de-authenticate then redirect due to being unauthorized
            await AccountService.LogoutGuiAsync(Guid.Empty);
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = await CurrentUserService.GetCurrentUserPrincipal();
        _canViewApi = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Api.View);
        _canViewJobs = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Jobs.View);
    }

    private async Task UpdateLoggedInUser()
    {
        var user = await CurrentUserService.GetCurrentUserFull();
        if (user is null)
            return;

        _loggedInUser = user;
    }
}