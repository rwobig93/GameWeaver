using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.Runtime;
using Application.Models.Identity.User;
using Application.Services.Lifecycle;
using Domain.Models.Identity;

namespace GameWeaver.Pages;

public partial class Index
{
    // MainLayout has a CascadingParameter of itself, this allows the refresh button on the AppBar to refresh all page state data
    //  If this parameter isn't cascaded to a page then the refresh button won't affect that pages' state data
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;

    private AppUserFull _loggedInUser = new();
    private AppUserPreferenceFull? _userPreferences;

    private string _cssBase = "rounded-lg pa-6 mt-12";
    private string _cssThemedBorder = "";
    private string _cssThemedText = "smaller";

    private bool _canViewApi;
    private bool _canViewJobs;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateLoggedInUser();
            UpdateThemedElements();
            await GetPermissions();
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = await CurrentUserService.GetCurrentUserPrincipal();
        _canViewApi = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Api.View);
        _canViewJobs = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Jobs.View);
    }

    private async Task UpdateLoggedInUser()
    {
        var user = await CurrentUserService.GetCurrentUserFull();
        if (user is null)
        {
            return;
        }

        _loggedInUser = user;

        var preferenceResponse = await AccountService.GetPreferences(_loggedInUser.Id);
        if (!preferenceResponse.Succeeded)
        {
            return;
        }

        _userPreferences = preferenceResponse.Data;
    }

    private void UpdateThemedElements()
    {
        if (_userPreferences is null)
        {
            return;
        }

        if (!_userPreferences.GamerMode)
        {
            return;
        }

        _cssThemedBorder = " border-rainbow";
        _cssThemedText = "smaller rainbow-text";
    }
}