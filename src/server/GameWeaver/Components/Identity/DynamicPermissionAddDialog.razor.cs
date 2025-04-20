using Application.Constants.Communication;
using Application.Mappers.Identity;
using Application.Models.Identity.Permission;
using Application.Responses.v1.Identity;
using Domain.Contracts;
using Domain.Enums.Identity;

namespace GameWeaver.Components.Identity;

public partial class DynamicPermissionAddDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; init; } = null!;
    [Inject] private IAppPermissionService PermissionService { get; init; } = null!;
    [Inject] private IAppRoleService RoleService { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;

    [Parameter] public Guid EntityId { get; set; }
    [Parameter] public DynamicPermissionGroup Group { get; set; }
    [Parameter] public bool CanPermissionEntity { get; set; }
    [Parameter] public bool IsForRolesNotUsers { get; set; } = true;

    private List<RoleResponse> _availableRoles = [];
    private RoleResponse? _selectedRole;
    private List<UserBasicResponse> _availableUsers = [];
    private UserBasicResponse? _selectedUser;
    private HashSet<AppPermissionCreate> _selectedPermissions = [];
    private List<AppPermissionSlim> _assignedPermissions = [];
    private List<AppPermissionCreate> _availablePermissions = [];

    private Guid _currentUserId;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();

            if (IsForRolesNotUsers)
            {
                await UpdateRoles();
            }
            else
            {
                await UpdateUsers();
            }

            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _currentUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
    }

    private async Task UpdateRoles(string searchText = "")
    {
        var rolesRequest = await RoleService.SearchPaginatedAsync(searchText, 1, 100);
        if (!rolesRequest.Succeeded)
        {
            rolesRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _availableRoles = rolesRequest.Data.ToResponses();
    }

    private async Task UpdateUsers(string searchText = "")
    {
        var usersRequest = await UserService.SearchPaginatedAsync(searchText, 1, 100);
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

        var assignedPermissions = await PermissionService.GetDynamicByTypeAndNameAsync(Group, EntityId);
        if (!assignedPermissions.Succeeded)
        {
            assignedPermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        _assignedPermissions = assignedPermissions.Data.Where(x => x.RoleId == _selectedRole.Id).ToList();

        var availablePermissions = Group switch
        {
            DynamicPermissionGroup.ServiceAccounts => await PermissionService.GetAllAvailableDynamicServiceAccountPermissionsAsync(EntityId),
            DynamicPermissionGroup.GameServers => await PermissionService.GetAllAvailableDynamicGameServerPermissionsAsync(EntityId),
            DynamicPermissionGroup.GameProfiles => await PermissionService.GetAllAvailableDynamicGameProfilePermissionsAsync(EntityId),
            _ => await Result<IEnumerable<AppPermissionCreate>>.FailAsync(ErrorMessageConstants.Permissions.DynamicPermissionNotSupported)
        };
        if (!availablePermissions.Succeeded)
        {
            availablePermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _availablePermissions = availablePermissions.Data.Where(x => _assignedPermissions
            .FirstOrDefault(p => p.ClaimValue == x.ClaimValue) is null).ToList();
    }

    private async Task<IEnumerable<RoleResponse>> FilterRoles(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText) || filterText.Length < 3)
        {
            return _availableRoles;
        }

        await UpdateRoles(filterText);
        return _availableRoles;
    }

    private async Task SelectedUserChanged(UserBasicResponse? user)
    {
        _selectedUser = user;
        if (_selectedUser is null)
        {
            return;
        }

        var assignedPermissions = await PermissionService.GetDynamicByTypeAndNameAsync(Group, EntityId);
        if (!assignedPermissions.Succeeded)
        {
            assignedPermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _assignedPermissions = assignedPermissions.Data.Where(x => x.UserId == _selectedUser.Id).ToList();
        var availablePermissions = Group switch
        {
            DynamicPermissionGroup.ServiceAccounts => await PermissionService.GetAllAvailableDynamicServiceAccountPermissionsAsync(EntityId),
            DynamicPermissionGroup.GameServers => await PermissionService.GetAllAvailableDynamicGameServerPermissionsAsync(EntityId),
            DynamicPermissionGroup.GameProfiles => await PermissionService.GetAllAvailableDynamicGameProfilePermissionsAsync(EntityId),
            _ => await Result<IEnumerable<AppPermissionCreate>>.FailAsync(ErrorMessageConstants.Permissions.DynamicPermissionNotSupported)
        };
        if (!availablePermissions.Succeeded)
        {
            availablePermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _availablePermissions = availablePermissions.Data.Where(x => _assignedPermissions
            .FirstOrDefault(p => p.ClaimValue == x.ClaimValue) is null).ToList();
    }

    private async Task<IEnumerable<UserBasicResponse>> FilterUsers(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText) || filterText.Length < 3)
        {
            return _availableUsers;
        }

        await UpdateUsers(filterText);
        return _availableUsers;
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
            if (permissionResponse.Succeeded) continue;

            permissionResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}