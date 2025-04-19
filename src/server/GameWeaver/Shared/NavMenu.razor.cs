using Application.Constants.Identity;
using Application.Helpers.Auth;

namespace GameWeaver.Shared;

public partial class NavMenu
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    private int _tooltipDelay = 500;
    private bool _canViewApi;
    private bool _canViewJobs;
    private bool _canViewUsers;
    private bool _canViewRoles;
    private bool _canViewAuditTrails;
    private bool _canViewTshootRecords;
    private bool _isDeveloper;
    private bool _canViewHosts;
    private bool _canSeeHostsUi;
    private bool _canViewGames;
    private bool _canViewGameServers;
    private bool _canSeeGameServersUi;

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
        if (currentUser is null)
        {
            return;
        }

        _canViewApi = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Api.View);
        _canViewJobs = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Jobs.View);
        _canViewUsers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.View);
        _canViewRoles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Roles.View);
        _canViewAuditTrails = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Audit.View);
        _canViewTshootRecords = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Troubleshooting.View);
        _isDeveloper = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.AppDevelopment.Dev);
        _canSeeHostsUi = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.SeeUi);
        _canViewHosts = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.Get);
        _canViewGames = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Get);
        _canSeeGameServersUi = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.SeeUi);
        _canViewGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
    }
}