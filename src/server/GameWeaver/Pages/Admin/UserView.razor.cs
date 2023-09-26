using System.Security.Claims;
using Application.Constants.Identity;
using Application.Helpers.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.User;
using Domain.Enums.Identity;
using GameWeaver.Components.Identity;

namespace GameWeaver.Pages.Admin;

public partial class UserView
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    [Parameter] public Guid UserId { get; set; }

    private ClaimsPrincipal _currentUser = new();
    private AppUserFull _viewingUser = new();
    private string? _createdByUsername = "";
    private string? _modifiedByUsername = "";
    private DateTime? _createdOn;
    private DateTime? _modifiedOn;
    private bool _processingEmailChange;

    private bool _invalidDataProvided;
    private bool _editMode;
    private bool _canEditUsers;
    private bool _canEnableUsers;
    private bool _canDisableUsers;
    private bool _canViewRoles;
    private bool _canAddRoles;
    private bool _canRemoveRoles;
    private bool _canViewPermissions;
    private bool _canAddPermissions;
    private bool _canRemovePermissions;
    private bool _canViewExtendedAttrs;
    private bool _canAdminServiceAccount;
    private bool _canAdminEmail;
    private bool _enableEditable;
    private string _editButtonText = "Enable Edit Mode";
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                ParseParametersFromUri();
                await GetClientTimezone();
                await GetViewingUser();
                await GetPermissions();
                StateHasChanged();
            }
        }
        catch
        {
            _invalidDataProvided = true;
            StateHasChanged();
        }
    }

    private void ParseParametersFromUri()
    {
        var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
        var queryParameters = QueryHelpers.ParseQuery(uri.Query);

        if (!queryParameters.TryGetValue("userId", out var queryUserId)) return;
        
        var providedIdIsValid = Guid.TryParse(queryUserId, out var parsedUserId);
        if (!providedIdIsValid)
            throw new InvalidDataException("Invalid UserId provided for user view");
            
        UserId = parsedUserId;
    }

    private async Task GetViewingUser()
    {
        _viewingUser = (await UserService.GetByIdFullAsync(UserId)).Data!;
        _createdByUsername = (await UserService.GetByIdAsync(_viewingUser.CreatedBy)).Data?.Username;
        _createdOn = _viewingUser.CreatedOn.ConvertToLocal(_localTimeZone);
        
        if (_viewingUser.LastModifiedBy is not null)
        {
            _modifiedByUsername = (await UserService.GetByIdAsync((Guid)_viewingUser.LastModifiedBy)).Data?.Username;
            _modifiedOn = ((DateTime) _viewingUser.LastModifiedOn!).ConvertToLocal(_localTimeZone);
        }
    }

    private async Task GetPermissions()
    {
        _currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canEditUsers = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Users.Edit);
        _canDisableUsers = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Users.Disable);
        _canEnableUsers = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Users.Enable);
        _canViewRoles = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Roles.View);
        _canAddRoles = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Roles.Add);
        _canRemoveRoles = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Roles.Remove);
        _canViewPermissions = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Permissions.View);
        _canAddPermissions = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Permissions.Add);
        _canRemovePermissions = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Permissions.Remove);
        _canViewExtendedAttrs = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Users.ViewExtAttrs);
        _canAdminEmail = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.Users.AdminEmail);
        if (_viewingUser.AccountType != AccountType.Service) return;
        
        _canAdminServiceAccount = await AuthorizationService.UserHasPermission(_currentUser, PermissionConstants.ServiceAccounts.Admin);
        if (_canAdminServiceAccount) return;

        // If not a service account admin check if the user has a dynamic permission to administrate this service account
        _canAdminServiceAccount = await AuthorizationService.UserHasPermission(_currentUser, PermissionHelpers.GetClaimValueFromServiceAccount(
            _viewingUser.Id, DynamicPermissionGroup.ServiceAccounts, DynamicPermissionLevel.Admin));
    }
    
    private async Task Save()
    {
        if (!_canEditUsers) return;
        
        var updateResult = await UserService.UpdateAsync(_viewingUser.ToUpdate(), CurrentUserService.GetIdFromPrincipal(_currentUser));
        if (!updateResult.Succeeded)
        {
            updateResult.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        var updateSecurityResult = await AccountService.SetAuthState(_viewingUser.Id, _viewingUser.AuthState);
        if (!updateSecurityResult.Succeeded)
        {
            updateSecurityResult.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        ToggleEditMode();
        await GetViewingUser();
        Snackbar.Add("Account successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private bool CanEditEnabled()
    {
        if (!_canDisableUsers && !_canEnableUsers) return false;
        if (_canDisableUsers && _canEnableUsers) return true;
        if (_canDisableUsers && _viewingUser.AuthState == AuthState.Enabled) return true;
        if (_canEnableUsers && _viewingUser.AuthState == AuthState.Disabled) return true;

        return false;
    }

    private void ToggleEditMode()
    {
        _editMode = !_editMode;

        _editButtonText = _editMode ? "Disable Edit Mode" : "Enable Edit Mode";
        _enableEditable = _editMode && CanEditEnabled();
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.Admin.Users);
    }

    private async Task EditRoles()
    {
        var dialogParameters = new DialogParameters() {{"UserId", _viewingUser.Id}};
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialog = await DialogService.Show<UserRoleDialog>("Edit User Roles", dialogParameters, dialogOptions).Result;
        if (!dialog.Canceled)
        {
            await GetViewingUser();
            StateHasChanged();
        }
    }

    private async Task EditPermissions()
    {
        var dialogParameters = new DialogParameters() {{"UserId", _viewingUser.Id}};
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialog = await DialogService.Show<UserPermissionDialog>("Edit User Permissions", dialogParameters, dialogOptions).Result;
        if (!dialog.Canceled)
        {
            await GetViewingUser();
            StateHasChanged();
        }
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private async Task EditServiceAccount()
    {
        if (_viewingUser.AccountType != AccountType.Service) return;
        
        if (!_canAdminServiceAccount)
        {
            Snackbar.Add("You don't have permission to edit service accounts, how'd you initiate this request!?", Severity.Error);
            return;
        }
        
        var updateParameters = new DialogParameters() { {"ServiceAccountId", _viewingUser.Id} };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var updateAccountDialog = await DialogService.Show<ServiceAccountAdminDialog>(
            "Update Service Account", updateParameters, dialogOptions).Result;
        if (updateAccountDialog.Canceled)
            return;

        var createdPassword = (string?) updateAccountDialog.Data;
        if (string.IsNullOrWhiteSpace(createdPassword))
        {
            StateHasChanged();
            return;
        }
        
        var copyParameters = new DialogParameters()
        {
            {"Title", "Please copy the account password and save it somewhere safe"},
            {"FieldLabel", "Service Account Password"},
            {"TextToDisplay", new string('*', createdPassword.Length)},
            {"TextToCopy", createdPassword}
        };
        await DialogService.Show<CopyTextDialog>("Service Account Password", copyParameters, dialogOptions).Result;
        await GetViewingUser();
        Snackbar.Add("Account successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private async Task ChangeEmail()
    {
        if (!_canAdminEmail) return;
        
        var dialogParameters = new DialogParameters()
        {
            {"Title", "Confirm New Email Address"},
            {"FieldLabel", "New Email Address"}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium, CloseOnEscapeKey = true };
        var newEmailPrompt = await DialogService.Show<ValuePromptDialog>("Confirm New Email", dialogParameters, dialogOptions).Result;
        if (newEmailPrompt.Canceled || string.IsNullOrWhiteSpace((string?)newEmailPrompt.Data))
            return;

        var newEmailAddress = (string)newEmailPrompt.Data;
        _processingEmailChange = true;
        StateHasChanged();
        var emailChangeRequest = await AccountService.InitiateEmailChange(_viewingUser.Id, newEmailAddress);
        if (!emailChangeRequest.Succeeded)
        {
            _processingEmailChange = false;
            StateHasChanged();
            emailChangeRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _processingEmailChange = false;
        StateHasChanged();
        Snackbar.Add(emailChangeRequest.Messages.First(), Severity.Success);
    }
}