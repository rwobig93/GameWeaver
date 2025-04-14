using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.Permission;
using Application.Responses.v1.Identity;
using Domain.Enums.Identity;

namespace GameWeaver.Components.GameServer;

public partial class GameServerPermissionAddDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; init; } = null!;
    [Inject] private IAppPermissionService PermissionService { get; init; } = null!;
    [Inject] private IAppRoleService RoleService { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;

    [Parameter] public Guid GameServerId { get; set; }
    [Parameter] public bool IsForRolesNotUsers { get; set; } = true;

    private List<RoleResponse> _availableRoles = [];
    private RoleResponse? _selectedRole;
    private List<UserBasicResponse> _availableUsers = [];
    private UserBasicResponse? _selectedUser;
    private HashSet<AppPermissionCreate> _selectedPermissions = [];
    private List<AppPermissionSlim> _assignedPermissions = [];
    private List<AppPermissionCreate> _availablePermissions = [];

    private Guid _currentUserId;
    private bool _canPermissionGameServer;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();

            if (IsForRolesNotUsers)
            {
                await GetRoles();
            }
            else
            {
                await GetUsers();
            }

            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _currentUserId = CurrentUserService.GetIdFromPrincipal(currentUser);

        var isServerAdmin = (await RoleService.IsUserAdminAsync(_currentUserId)).Data || await AuthorizationService.UserHasPermission(currentUser,
            PermissionConstants.GameServer.Gameserver.Dynamic(GameServerId, DynamicPermissionLevel.Admin));
        var isServerModerator = (await RoleService.IsUserModeratorAsync(_currentUserId)).Data || await AuthorizationService.UserHasPermission(currentUser,
            PermissionConstants.GameServer.Gameserver.Dynamic(GameServerId, DynamicPermissionLevel.Moderator));

        _canPermissionGameServer = isServerAdmin || isServerModerator ||
            await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Dynamic(GameServerId, DynamicPermissionLevel.Permission));
    }

    private async Task GetRoles()
    {
        var rolesRequest = await RoleService.GetAllAsync();
        if (!rolesRequest.Succeeded)
        {
            rolesRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _availableRoles = rolesRequest.Data.ToResponses();
    }

    private async Task GetUsers()
    {
        var usersRequest = await UserService.GetAllAsync();
        if (!usersRequest.Succeeded)
        {
            usersRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _availableUsers = usersRequest.Data.ToResponses();
    }

    private async Task SelectedRoleChanged(RoleResponse? role)
    {
        _selectedRole = role;
        if (_selectedRole is null)
        {
            return;
        }

        var assignedServerPermissions = await PermissionService.GetDynamicByTypeAndNameAsync(DynamicPermissionGroup.GameServers, GameServerId);
        if (!assignedServerPermissions.Succeeded)
        {
            assignedServerPermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        _assignedPermissions = assignedServerPermissions.Data.Where(x => x.RoleId == _selectedRole.Id).ToList();

        var availableServerPermissions = await PermissionService.GetAllAvailableDynamicGameServerPermissionsAsync(GameServerId);
        if (!availableServerPermissions.Succeeded)
        {
            availableServerPermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        _availablePermissions = availableServerPermissions.Data.Where(x => _assignedPermissions
            .FirstOrDefault(p => p.ClaimValue == x.ClaimValue) is null).ToList();
    }

    private async Task<IEnumerable<RoleResponse>> FilterRoles(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _availableRoles;
        }

        await Task.CompletedTask;

        return _availableRoles.Where(x =>
            x.Name.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Id.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task SelectedUserChanged(UserBasicResponse? user)
    {
        _selectedUser = user;
        if (_selectedUser is null)
        {
            return;
        }

        var assignedServerPermissions = await PermissionService.GetDynamicByTypeAndNameAsync(DynamicPermissionGroup.GameServers, GameServerId);
        if (!assignedServerPermissions.Succeeded)
        {
            assignedServerPermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        _assignedPermissions = assignedServerPermissions.Data.Where(x => x.UserId == _selectedUser.Id).ToList();

        var availableServerPermissions = await PermissionService.GetAllAvailableDynamicGameServerPermissionsAsync(GameServerId);
        if (!availableServerPermissions.Succeeded)
        {
            availableServerPermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        _availablePermissions = availableServerPermissions.Data.Where(x => _assignedPermissions
            .FirstOrDefault(p => p.ClaimValue == x.ClaimValue) is null).ToList();
    }

    private async Task<IEnumerable<UserBasicResponse>> FilterUsers(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _availableUsers;
        }

        await Task.CompletedTask;

        return _availableUsers.Where(x =>
            x.Username.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Id.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task Save()
    {
        if (_selectedPermissions.Count == 0)
        {
            Snackbar.Add("No permissions selected to add", Severity.Error);
            return;
        }

        foreach (var permission in _selectedPermissions)
        {
            if (_selectedUser is not null)
            {
                permission.UserId = _selectedUser.Id;
            }

            if (_selectedRole is not null)
            {
                permission.RoleId = _selectedRole.Id;
            }

            var permissionResponse = await PermissionService.CreateAsync(permission, _currentUserId);
            if (!permissionResponse.Succeeded)
            {
                permissionResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}