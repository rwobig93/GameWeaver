using Application.Constants.Identity;
using Application.Helpers.Runtime;

namespace GameWeaver.Shared;

public partial class NavMenu
{
    private bool _canViewApi;
    private bool _canViewJobs;
    private bool _canViewUsers;
    private bool _canViewRoles;
    private bool _canViewAuditTrails;
    private bool _isDeveloper;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = await CurrentUserService.GetCurrentUserPrincipal();
        _canViewApi = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Api.View);
        _canViewJobs = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Jobs.View);
        _canViewUsers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.View);
        _canViewRoles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Roles.View);
        _canViewAuditTrails = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Audit.View);
        _isDeveloper = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.AppDevelopment.Dev);
    }
}