using Application.Constants.Identity;
using Application.Helpers.Identity;
using Application.Helpers.Runtime;
using Application.Helpers.Web;
using Application.Mappers.Identity;
using Application.Models.Identity.User;
using Application.Responses.v1.Identity;
using Domain.Enums.Identity;

namespace GameWeaver.Components.Identity;

public partial class ServiceAccountAdminDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; init; } = null!;
    [Parameter] public Guid ServiceAccountId { get; set; } = Guid.Empty;
    
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IAppRoleService RoleService { get; init; } = null!;

    private UserBasicResponse _currentUser = new();
    private AppUserSecurityFull _serviceUser = new() { Id = Guid.Empty };
    private readonly PasswordRequirementsResponse _passwordRequirements = AccountHelpers.GetPasswordRequirements();
    private string _desiredPassword = "";
    private string _confirmPassword = "";
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private InputType _passwordConfirmInput = InputType.Password;
    private string _passwordConfirmInputIcon = Icons.Material.Filled.VisibilityOff;
    private bool _creatingServiceAccount;

    private bool _canAdminServiceAccounts;
    

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetCurrentUser();
            await GetPermissions();
            await GetServiceAccount();
            ValidateActionState();
            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        var foundUser = await CurrentUserService.GetCurrentUserBasic();
        if (foundUser is null)
            return;

        _currentUser = foundUser;
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canAdminServiceAccounts = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.ServiceAccounts.Admin);
    }

    private async Task GetServiceAccount()
    {
        if (ServiceAccountId == Guid.Empty) return;

        var getAccountRequest = await UserService.GetByIdSecurityFullAsync(ServiceAccountId);
        if (!getAccountRequest.Succeeded || getAccountRequest.Data is null)
        {
            getAccountRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            _serviceUser.Id = Guid.Empty;
            return;
        }

        _serviceUser = getAccountRequest.Data;
    }

    private void ValidateActionState()
    {
        if (_serviceUser.Id == Guid.Empty)
            _creatingServiceAccount = true;

        if (!_creatingServiceAccount) return;
        
        _desiredPassword = UrlHelpers.GenerateToken(64);
        _confirmPassword = _desiredPassword;
        _serviceUser.Notes = $"Service Account - {DateTimeService.NowDatabaseTime.ToFriendlyDisplayMilitaryTimezone()}";
    }
    
    private IEnumerable<string> ValidatePasswordRequirements(string content)
    {
        var passwordIssues = AccountHelpers.GetAnyIssuesWithPassword(content);
        if (!string.IsNullOrEmpty(content) && passwordIssues.Any())
            yield return passwordIssues.First();
    }
    
    private IEnumerable<string> ValidatePasswordsMatch(string content)
    {
        if (!string.IsNullOrEmpty(content) &&
            !string.IsNullOrWhiteSpace(_desiredPassword) &&
            content != _desiredPassword)
            yield return "Desired & Confirm Passwords Don't Match";
    }

    private void TogglePasswordVisibility()
    {
        if (_passwordInputIcon == Icons.Material.Filled.VisibilityOff)
        {
            _passwordInput = InputType.Text;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            return;
        }

        _passwordInput = InputType.Password;
        _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    }

    private void ToggleConfirmPasswordVisibility()
    {
        if (_passwordConfirmInputIcon == Icons.Material.Filled.VisibilityOff)
        {
            _passwordConfirmInput = InputType.Text;
            _passwordConfirmInputIcon = Icons.Material.Filled.Visibility;
            return;
        }

        _passwordConfirmInput = InputType.Password;
        _passwordConfirmInputIcon = Icons.Material.Filled.VisibilityOff;
    }

    private bool RequirementsAreMet()
    {
        // If editing a service account and we aren't changing a password we're good to go
        if (!_creatingServiceAccount && string.IsNullOrWhiteSpace(_desiredPassword) && string.IsNullOrWhiteSpace(_confirmPassword)) return true;

        if (string.IsNullOrWhiteSpace(_serviceUser.Username) || _serviceUser.Username.Length < 3)
        {
            Snackbar.Add("Username is too short, please have at least 3 characters and try again", Severity.Error);
            return false;
        }
        
        var passwordIssues = AccountHelpers.GetAnyIssuesWithPassword(_desiredPassword);
        if (!string.IsNullOrEmpty(_desiredPassword) && passwordIssues.Any())
        {
            Snackbar.Add(passwordIssues.First(), Severity.Error);
            return false;
        }
        
        if (_desiredPassword != _confirmPassword)
        {
            Snackbar.Add("Desired & Confirm Passwords Don't Match", Severity.Error);
            return false;
        }

        return true;
    }
    
    private async Task Save()
    {
        if (!_canAdminServiceAccounts) return;
        
        if (!RequirementsAreMet()) return;

        _serviceUser.AccountType = AccountType.Service;
        
        if (_creatingServiceAccount)
        {
            _serviceUser.CreatedBy = _currentUser.Id;
            _serviceUser.Email = $"{Guid.NewGuid()}@Service.Account";
            _serviceUser.EmailConfirmed = true;
            _serviceUser.AuthState = AuthState.Enabled;
            var createUserRequest = await UserService.CreateAsync(_serviceUser.ToCreate(), _currentUser.Id);
            if (!createUserRequest.Succeeded)
            {
                createUserRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            _serviceUser.Id = createUserRequest.Data;
            var getServiceAccountRoleRequest = await RoleService.GetByNameAsync(RoleConstants.DefaultRoles.ServiceAccountName);
            if (!getServiceAccountRoleRequest.Succeeded || getServiceAccountRoleRequest.Data is null)
            {
                getServiceAccountRoleRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
            
            var addRoleRequest = await RoleService.AddUserToRoleAsync(
                _serviceUser.Id, getServiceAccountRoleRequest.Data.Id, _currentUser.Id);
            if (!addRoleRequest.Succeeded)
            {
                addRoleRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            var enableRequest = await AccountService.SetAuthState(_serviceUser.Id, AuthState.Enabled);
            if (!enableRequest.Succeeded)
            {
                enableRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            Snackbar.Add("Successfully created Service Account!", Severity.Success);
            MudDialog.Close(DialogResult.Ok(_desiredPassword));
        }

        if (!string.IsNullOrWhiteSpace(_desiredPassword) && !string.IsNullOrWhiteSpace(_confirmPassword))
        {
            var updatePasswordRequest = await AccountService.SetUserPassword(_serviceUser.Id, _desiredPassword);
            if (!updatePasswordRequest.Succeeded)
            {
                updatePasswordRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }
        
        if (_creatingServiceAccount) return;

        _serviceUser.LastModifiedBy = _currentUser.Id;
        var updateAccountRequest = await UserService.UpdateAsync(_serviceUser.ToUserUpdate(), _currentUser.Id);
        if (!updateAccountRequest.Succeeded)
        {
            updateAccountRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Successfully updated service account!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(_desiredPassword));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}