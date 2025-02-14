using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.Role;

namespace GameWeaver.Components.Identity;

public partial class RoleCreateDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; init; } = null!;

    [Inject] private IAppRoleService RoleService { get; init; } = null!;

    private Guid _currentUserId;
    private AppRoleCreate _newRole = new();

    private bool _canCreateRoles;
    
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
        _currentUserId = (await CurrentUserService.GetCurrentUserId()).GetFromNullable();
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canCreateRoles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Roles.Create);
    }
    
    private async Task Save()
    {
        if (!_canCreateRoles) return;
        
        var createRoleRequest = await RoleService.CreateAsync(_newRole, _currentUserId);
        if (!createRoleRequest.Succeeded)
        {
            createRoleRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        Snackbar.Add("Successfully created role!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(createRoleRequest.Data));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}