using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.Permission;

namespace GameWeaver.Components.Identity;

public partial class RolePermissionDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; init; } = null!;
    [Inject] private IAppPermissionService PermissionService { get; init; } = null!;

    [Parameter] public Guid RoleId { get; set; }

    private List<AppPermissionSlim> _assignedPermissions = [];
    private List<AppPermissionCreate> _availablePermissions = [];
    private HashSet<AppPermissionCreate> _addPermissions = [];
    private HashSet<AppPermissionSlim> _removePermissions = [];
    private Guid _currentUserId;
    private bool _canRemovePermissions;
    private bool _canAddPermissions;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetPermissionLists();
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _currentUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canRemovePermissions = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Permissions.Remove);
        _canAddPermissions = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Permissions.Add);
    }

    private async Task GetPermissionLists()
    {
        if (!_canRemovePermissions && !_canAddPermissions)
        {
            return;
        }

        var rolePermissions = await PermissionService.GetAllForRoleAsync(RoleId);
        if (!rolePermissions.Succeeded)
        {
            rolePermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (_canRemovePermissions)
        {
            _assignedPermissions = rolePermissions.Data
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access)
                .ToList();
        }

        if (_canAddPermissions)
        {
            var allPermissions = await PermissionService.GetAllAvailablePermissionsAsync();
            if (!allPermissions.Succeeded)
            {
                allPermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            _availablePermissions = allPermissions.Data
                .Except(rolePermissions.Data.ToCreates(), new PermissionCreateComparer())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access)
                .ToList();
        }
    }

    private readonly TableGroupDefinition<AppPermissionSlim> _groupDefinitionDb = new()
    {
        GroupName = "Category",
        Indentation = false,
        Expandable = true,
        IsInitiallyExpanded = false,
        Selector = (p) => p.Name
    };

    private readonly TableGroupDefinition<AppPermissionCreate> _groupDefinitionCreate = new()
    {
        GroupName = "Category",
        Indentation = false,
        Expandable = true,
        IsInitiallyExpanded = false,
        Selector = (p) => p.Name
    };

    private void AddPermissions()
    {
        if (!_canAddPermissions) return;

        foreach (var permission in _addPermissions)
        {
            _availablePermissions.Remove(permission);

            var permissionConverted = permission.ToSlim();
            permissionConverted.CreatedBy = _currentUserId;

            _assignedPermissions.Add(permissionConverted);
        }
    }

    private void RemovePermissions()
    {
        if (!_canRemovePermissions) return;

        foreach (var permission in _removePermissions)
        {
            _assignedPermissions.Remove(permission);

            var permissionConverted = permission.ToCreate();
            permissionConverted.CreatedBy = _currentUserId;

            _availablePermissions.Add(permissionConverted);
        }
    }

    private async Task Save()
    {
        if (!_canAddPermissions && !_canRemovePermissions) return;

        var currentPermissions = await PermissionService.GetAllForRoleAsync(RoleId);
        if (!currentPermissions.Succeeded)
        {
            currentPermissions.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var permission in _assignedPermissions.Where(perm => currentPermissions.Data.All(x => x.Id != perm.Id)))
        {
            var addPermission = await PermissionService.CreateAsync(new AppPermissionCreate
            {
                RoleId = RoleId,
                ClaimType = permission.ClaimType ?? "",
                ClaimValue = permission.ClaimValue,
                Name = permission.Name,
                Group = permission.Group,
                Access = permission.Access,
                Description = permission.Description,
                CreatedBy = _currentUserId
            }, _currentUserId);

            if (!addPermission.Succeeded)
            {
                addPermission.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                continue;
            }

            Snackbar.Add($"Successfully added permission {permission.Group}.{permission.Name}.{permission.Access}", Severity.Success);
        }

        foreach (var permission in currentPermissions.Data.Where(perm => _assignedPermissions.All(x => x.Id != perm.Id)))
        {
            var removePermission = await PermissionService.DeleteAsync(permission.Id, _currentUserId);
            if (!removePermission.Succeeded)
            {
                removePermission.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                continue;
            }

            Snackbar.Add($"Successfully removed permission {permission.Group}.{permission.Name}.{permission.Access}", Severity.Success);
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}