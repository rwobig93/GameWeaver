using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Models.Identity.Role;

namespace GameWeaver.Components.Identity;

public partial class UserRoleDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; init; } = null!;
    [Inject] private IAppRoleService RoleService { get; init; } = null!;

    [Parameter] public Guid UserId { get; set; }

    private Guid _currentUserId;
    private List<AppRoleSlim> _assignedRoles = [];
    private List<AppRoleSlim> _availableRoles = [];
    private HashSet<AppRoleSlim> _addRoles = [];
    private HashSet<AppRoleSlim> _removeRoles = [];
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

        var userRoles = await RoleService.GetRolesForUser(UserId);
        if (!userRoles.Succeeded)
        {
            userRoles.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (_canRemoveRoles)
        {
            _assignedRoles = userRoles.Data.ToList();
        }

        if (_canAddRoles)
        {
            var allRoles = await RoleService.GetAllAsync();
            if (!allRoles.Succeeded)
            {
                allRoles.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            _availableRoles = allRoles.Data.Where(x => userRoles.Data.All(r => r.Id != x.Id)).ToList();
        }
    }

    private void AddRoles()
    {
        if (!_canAddRoles) return;

        foreach (var role in _addRoles)
        {
            _availableRoles.Remove(role);
            _assignedRoles.Add(role);
        }
    }

    private void RemoveRoles()
    {
        if (!_canRemoveRoles) return;

        foreach (var role in _removeRoles)
        {
            _assignedRoles.Remove(role);
            _availableRoles.Add(role);
        }
    }

    private async Task Save()
    {
        if (!_canAddRoles && !_canRemoveRoles) return;

        var currentRoles = await RoleService.GetRolesForUser(UserId);
        if (!currentRoles.Succeeded)
        {
            currentRoles.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var role in _assignedRoles.Where(role => currentRoles.Data.All(x => x.Id != role.Id)))
        {
            var addRole = await RoleService.AddUserToRoleAsync(UserId, role.Id, _currentUserId);
            if (!addRole.Succeeded)
            {
                addRole.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                continue;
            }

            Snackbar.Add($"Successfully added role {role.Name}", Severity.Success);
        }

        foreach (var role in currentRoles.Data.Where(role => _assignedRoles.All(x => x.Id != role.Id)))
        {
            var removeRole = await RoleService.RemoveUserFromRoleAsync(UserId, role.Id, _currentUserId);
            if (!removeRole.Succeeded)
            {
                removeRole.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                continue;
            }

            Snackbar.Add($"Successfully removed role {role.Name}", Severity.Success);
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}