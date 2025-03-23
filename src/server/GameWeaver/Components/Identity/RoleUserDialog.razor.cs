using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.User;

namespace GameWeaver.Components.Identity;

public partial class RoleUserDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IAppRoleService RoleService { get; init; } = null!;

    [Parameter] public Guid RoleId { get; set; }

    private Guid _currentUserId;
    private List<AppUserSlim> _assignedUsers = [];
    private List<AppUserSlim> _availableUsers = [];
    private HashSet<AppUserSlim> _addUsers = [];
    private HashSet<AppUserSlim> _removeUsers = [];
    private bool _canRemoveRoles;
    private bool _canAddRoles;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetRoleLists();
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _currentUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canRemoveRoles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Roles.Remove);
        _canAddRoles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Roles.Add);
    }

    private async Task GetRoleLists()
    {
        if (!_canRemoveRoles && !_canAddRoles)
            return;

        var roleUsers = await RoleService.GetUsersForRole(RoleId);
        if (!roleUsers.Succeeded)
        {
            roleUsers.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (_canRemoveRoles)
        {
            _assignedUsers = roleUsers.Data.ToList();
        }

        if (_canAddRoles)
        {
            var allUsers = await UserService.GetAllAsync();
            if (!allUsers.Succeeded)
            {
                allUsers.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            _availableUsers = allUsers.Data.Where(x => roleUsers.Data.All(r => r.Id != x.Id)).ToList();
        }
    }

    private void AddUsers()
    {
        if (!_canAddRoles) return;
        
        foreach (var user in _addUsers)
        {
            _availableUsers.Remove(user);
            _assignedUsers.Add(user);
        }
    }

    private void RemoveUsers()
    {
        if (!_canRemoveRoles) return;
        
        foreach (var user in _removeUsers)
        {
            _assignedUsers.Remove(user);
            _availableUsers.Add(user);
        }
    }
    
    private async Task Save()
    {
        if (!_canAddRoles && !_canRemoveRoles) return;
        
        var currentUsers = await RoleService.GetUsersForRole(RoleId);
        if (!currentUsers.Succeeded)
        {
            currentUsers.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var user in _assignedUsers.Where(role => currentUsers.Data.All(x => x.Id != role.Id)))
        {
            var addRole = await RoleService.AddUserToRoleAsync(user.Id, RoleId, _currentUserId);
            if (!addRole.Succeeded)
            {
                addRole.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                continue;
            }

            Snackbar.Add($"Successfully added user {user.Username}", Severity.Success);
        }

        foreach (var user in currentUsers.Data.Where(user => _assignedUsers.All(x => x.Id != user.Id)))
        {
            var removeRole = await RoleService.RemoveUserFromRoleAsync(user.Id, RoleId, _currentUserId);
            if (!removeRole.Succeeded)
            {
                removeRole.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                continue;
            }

            Snackbar.Add($"Successfully removed user {user.Username}", Severity.Success);
        }
        
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}