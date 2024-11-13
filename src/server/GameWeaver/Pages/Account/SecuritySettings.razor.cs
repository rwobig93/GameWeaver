using Application.Constants.Identity;
using Application.Helpers.Identity;
using Application.Helpers.Integrations;
using Application.Helpers.Runtime;
using Application.Models.Identity.User;
using Application.Models.Identity.UserExtensions;
using Application.Responses.v1.Identity;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using Application.Settings.AppSettings;
using Domain.Enums.Identity;
using Domain.Enums.Integrations;
using Microsoft.Extensions.Options;
using GameWeaver.Components.Account;

namespace GameWeaver.Pages.Account;

public partial class SecuritySettings
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    [Parameter] public string OauthCode { get; set; } = "";
    [Parameter] public string OauthState { get; set; } = "";
    
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;
    [Inject] private IQrCodeService QrCodeService { get; init; } = null!;
    [Inject] private IMfaService MfaService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IExternalAuthProviderService ExternalAuthService { get; init; } = null!;
    [Inject] private IOptions<AppConfiguration> AppConfig { get; init; } = null!;

    private AppUserSecurityFull CurrentUser { get; set; } = new();
    
    private bool _canGenerateApiTokens;
    private MudTabs _securityTabs = null!;
    private MudTabPanel _externalAuthPanel = null!;
    
    // User Password Change
    private string CurrentPassword { get; set; } = "";
    private string DesiredPassword { get; set; } = "";
    private string ConfirmPassword { get; set; } = "";
    private readonly PasswordRequirementsResponse _passwordRequirements = AccountHelpers.GetPasswordRequirements();
    private InputType _passwordCurrentInput = InputType.Password;
    private string _passwordCurrentInputIcon = Icons.Material.Filled.VisibilityOff;
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private InputType _passwordConfirmInput = InputType.Password;
    private string _passwordConfirmInputIcon = Icons.Material.Filled.VisibilityOff;

    // MFA
    private string _mfaButtonText = "";
    private string _mfaRegisterCode = "";
    private string _qrCodeImageSource = "";
    private string _totpCode = "";
    private bool QrCodeGenerating { get; set; }
    
    // User API Tokens
    private List<AppUserExtendedAttributeSlim> _userApiTokens = [];
    private List<AppUserExtendedAttributeSlim> _userClientSessions = [];
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private HashSet<AppUserExtendedAttributeSlim> _selectedApiTokens = [];
    
    // External Auth
    private bool _linkedAuthGoogle;
    private bool _linkedAuthDiscord;
    private bool _linkedAuthSpotify;
    

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ParseParametersFromUri();
            await GetCurrentUser();
            await GetPermissions();
            await GetClientTimezone();
            await GetUserApiTokens();
            await GetUserClientSessions();
            await GetUserExternalAuthLinks();
            await UpdatePageElementStates();
            await HandleExternalLoginRedirect();
            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        var foundUser = await CurrentUserService.GetCurrentUserSecurityFull();
        if (foundUser is null)
            return;

        CurrentUser = foundUser;
    }

    private void ParseParametersFromUri()
    {
        var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
        var queryParameters = QueryHelpers.ParseQuery(uri.Query);
        
        if (queryParameters.TryGetValue(LoginRedirectConstants.OauthCode, out var oauthCode))
            OauthCode = oauthCode!;
        
        if (queryParameters.TryGetValue(LoginRedirectConstants.OauthState, out var oauthState))
            OauthState = oauthState!;
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canGenerateApiTokens = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Api.GenerateToken);
    }

    private async Task UpdatePageElementStates()
    {
        _mfaButtonText = CurrentUser.TwoFactorEnabled switch
        {
            true => "Disable MFA",
            false when !string.IsNullOrWhiteSpace(CurrentUser.TwoFactorKey) => "Enable MFA",
            _ => "Register MFA TOTP Token"
        };
        await Task.CompletedTask;
    }

    private async Task UpdatePassword()
    {
        if (!await IsRequiredInformationPresent())
            return;

        await AccountService.SetUserPassword(CurrentUser.Id, DesiredPassword);

        Snackbar.Add("Password successfully changed!");
        StateHasChanged();
    }

    private void ToggleCurrentPasswordVisibility()
    {
        if (_passwordCurrentInputIcon == Icons.Material.Filled.VisibilityOff)
        {
            _passwordCurrentInput = InputType.Text;
            _passwordCurrentInputIcon = Icons.Material.Filled.Visibility;
            return;
        }

        _passwordCurrentInput = InputType.Password;
        _passwordCurrentInputIcon = Icons.Material.Filled.VisibilityOff;
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

    private async Task<bool> IsRequiredInformationPresent()
    {
        var informationValid = true;

        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            Snackbar.Add("Current Password field is empty", Severity.Error);
            informationValid = false;
        }

        if (string.IsNullOrWhiteSpace(DesiredPassword))
        {
            Snackbar.Add("Desired Password field is empty", Severity.Error);
            informationValid = false;
        }

        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            Snackbar.Add("Confirm Password field is empty", Severity.Error);
            informationValid = false;
        }

        if (DesiredPassword != ConfirmPassword)
        {
            Snackbar.Add("Passwords provided don't match", Severity.Error);
            informationValid = false;
        }

        if (!(await AccountService.IsPasswordCorrect(CurrentUser.Id, CurrentPassword)).Data)
        {
            Snackbar.Add("Current password provided is incorrect", Severity.Error);
            informationValid = false;
        }

        if (!(await AccountService.PasswordMeetsRequirements(DesiredPassword)).Data)
        {
            Snackbar.Add("Desired password doesn't meet the password requirements", Severity.Error);
            informationValid = false;
        }

        return informationValid;
    }

    private async Task InvokeTotpAction()
    {
        // If the account doesn't have a MFA key we want to allow registering
        if (string.IsNullOrWhiteSpace(CurrentUser.TwoFactorKey))
        {
            await RegisterTotp();
            return;
        }

        // If we have a MFA key on the account we want to allow toggling MFA on/off for the account
        await ToggleMfaEnablement(!CurrentUser.TwoFactorEnabled);
    }
    
    private async Task RegisterTotp()
    {
        QrCodeGenerating = true;
        
        try
        {
            _mfaRegisterCode = MfaService.GenerateKeyString();
            var appName = ServerState.ApplicationName;
            var qrCodeContent =
                MfaService.GenerateOtpAuthString(appName, CurrentUser.Email, _mfaRegisterCode);
            _qrCodeImageSource = QrCodeService.GenerateQrCodeSrc(qrCodeContent);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to generate TOTP Registration: {ex.Message}", Severity.Error);
        }
        
        QrCodeGenerating = false;
        await Task.CompletedTask;
        StateHasChanged();
    }

    private async Task ToggleMfaEnablement(bool enabled)
    {
        var result = await AccountService.SetTwoFactorEnabled(CurrentUser.Id, enabled);
        if (!result.Succeeded)
        {
            result.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        await GetCurrentUser();
        StateHasChanged();
        var mfaEnablement = CurrentUser.TwoFactorEnabled ? "Enabled" : "Disabled";
        Snackbar.Add($"Successfully toggled MFA to {mfaEnablement}");
        await UpdatePageElementStates();
    }

    private async Task ValidateTotpCode()
    {
        var totpCorrect = MfaService.IsPasscodeCorrect(_totpCode, _mfaRegisterCode, out _);
        if (!totpCorrect)
        {
            Snackbar.Add("TOTP code provided is incorrect, please try again", Severity.Error);
            return;
        }
        
        await AccountService.SetTwoFactorKey(CurrentUser.Id, _mfaRegisterCode);
        await AccountService.SetTwoFactorEnabled(CurrentUser.Id, true);
        
        _mfaRegisterCode = "";
        _qrCodeImageSource = "";
        Snackbar.Add("TOTP code provided is correct!", Severity.Success);
        
        // Wait for the snackbar message to be read then we reload the page to force page elements to update
        //  would love to find a better solution for this but as of now StateHasChanged or force updating doesn't work
        await Task.Delay(TimeSpan.FromSeconds(3));
        
        NavManager.NavigateTo(AppRouteConstants.Account.Security, true);
    }

    private async Task TotpSubmitCheck(KeyboardEventArgs arg)
    {
        if (arg.Key == "Enter")
            await ValidateTotpCode();
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private async Task GetUserApiTokens()
    {
        if (!_canGenerateApiTokens) return;

        var tokensRequest =
            await UserService.GetUserExtendedAttributesByTypeAsync(CurrentUser.Id, ExtendedAttributeType.UserApiToken);
        if (!tokensRequest.Succeeded)
        {
            tokensRequest.Messages.ForEach(x => Snackbar.Add($"Api Token retrieval failed: {x}", Severity.Error));
            return;
        }

        _userApiTokens = tokensRequest.Data.ToList();
    }

    private async Task GetUserClientSessions()
    {
        var clientSessionsRequest = await UserService.GetUserExtendedAttributesByTypeAsync(CurrentUser.Id, ExtendedAttributeType.UserClientId);
        if (!clientSessionsRequest.Succeeded)
        {
            clientSessionsRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _userClientSessions = clientSessionsRequest.Data.ToList();
    }

    private async Task GetUserExternalAuthLinks()
    {
        if (!ExternalAuthService.AnyProvidersEnabled) return;

        var externalAuthRequest =
            await UserService.GetUserExtendedAttributesByTypeAsync(CurrentUser.Id, ExtendedAttributeType.ExternalAuthLogin);
        if (!externalAuthRequest.Succeeded)
        {
            externalAuthRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        if (!externalAuthRequest.Data.Any())
        {
            _linkedAuthDiscord = false;
            _linkedAuthGoogle = false;
            _linkedAuthSpotify = false;
            return;
        }

        _linkedAuthDiscord = externalAuthRequest.Data.Any(x => x.Description == ExternalAuthProvider.Discord.ToString());
        _linkedAuthGoogle = externalAuthRequest.Data.Any(x => x.Description == ExternalAuthProvider.Google.ToString());
        _linkedAuthSpotify = externalAuthRequest.Data.Any(x => x.Description == ExternalAuthProvider.Spotify.ToString());
    }

    private async Task GenerateUserApiToken()
    {
        if (!_canGenerateApiTokens) return;
        
        var dialogParameters = new DialogParameters() {{"ApiTokenId", Guid.Empty}};
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        await DialogService.Show<UserApiTokenDialog>("Create API Token", dialogParameters, dialogOptions).Result;

        await GetUserApiTokens();
        StateHasChanged();
    }

    private async Task UpdateApiToken()
    {
        if (!_canGenerateApiTokens) return;
        if (_selectedApiTokens.Count != 1) return;
        
        var dialogParameters = new DialogParameters() {{"ApiTokenId", _selectedApiTokens.FirstOrDefault()!.Id}};
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium, CloseOnEscapeKey = true };
        await DialogService.Show<UserApiTokenDialog>("Update API Token", dialogParameters, dialogOptions).Result;
        
        await GetUserApiTokens();
        StateHasChanged();
    }

    private async Task DeleteApiTokens()
    {
        if (!_canGenerateApiTokens) return;
        
        var tokensList = _selectedApiTokens.Select(x => $"Token: [{x.Value[^4..]}] {x.Description}").ToArray();
        
        var dialogParameters = new DialogParameters()
        {
            {"Title", $"Are you sure you want to delete these {_selectedApiTokens.Count} API Tokens?"},
            {"Content", string.Join(Environment.NewLine, tokensList)}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium, CloseOnEscapeKey = true };
        var confirmation = await DialogService.Show<ConfirmationDialog>("Confirm Deletion", dialogParameters, dialogOptions).Result;
        if (confirmation?.Data is null || confirmation.Canceled)
        {
            return;
        }

        var messages = new List<string>();
        
        foreach (var token in _selectedApiTokens)
        {
            var tokenDeleteRequest = await AccountService.DeleteUserApiToken(CurrentUser.Id, token.Value);
            if (!tokenDeleteRequest.Succeeded)
                messages.Add(tokenDeleteRequest.Messages.First());
        }

        if (messages.Any())
        {
            messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        Snackbar.Add("Finished deleting selected API tokens", Severity.Success);
        await GetUserApiTokens();
        StateHasChanged();
    }

    private async Task CopyToClipboard(string content)
    {
        var copyRequest = await WebClientService.InvokeClipboardCopy(content);
        if (!copyRequest.Succeeded)
        {
            copyRequest.Messages.ForEach(x => Snackbar.Add($"Failed to copy to clipboard: {x}", Severity.Error));
            return;
        }

        Snackbar.Add("Successfully copied API token to your clipboard!", Severity.Info);
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
            !string.IsNullOrWhiteSpace(DesiredPassword) &&
            content != DesiredPassword)
            yield return "Desired & Confirm Passwords Don't Match";
    }

    private async Task HandleExternalAuthLinking(ExternalAuthProvider provider, bool accountIsLinked)
    {
        // Account is linked so we'll unlink/remove the account
        if (accountIsLinked)
        {
            var removeRequest = await AccountService.RemoveExternalAuthProvider(CurrentUser.Id, provider);
            if (!removeRequest.Succeeded)
            {
                removeRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            Snackbar.Add($"Successfully unlinked your {provider} account", Severity.Success);
            await GetUserExternalAuthLinks();
            StateHasChanged();
            return;
        }
        
        // Account is not linked so we'll start linking - initiate a redirect to the provider, on successful auth we'll link the account
        var loginUriRedirectRequest = await ExternalAuthService.GetLoginUri(provider, ExternalAuthRedirect.Security);
        if (!loginUriRedirectRequest.Succeeded)
        {
            loginUriRedirectRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        NavManager.NavigateTo(loginUriRedirectRequest.Data);
    }

    private async Task HandleExternalLoginRedirect()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OauthCode) || string.IsNullOrWhiteSpace(OauthState)) return;

            var provider = ExternalAuthHelpers.StringToProvider(OauthCode);

            var addExternalAuthRequest = await AccountService.SetExternalAuthProvider(CurrentUser.Id, provider, OauthState);
            if (!addExternalAuthRequest.Succeeded)
            {
                addExternalAuthRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
            
            Snackbar.Add($"Your {AppConfig.Value.ApplicationName} account has been linked to your {provider} account!", Severity.Success);
            await GetUserExternalAuthLinks();
            _securityTabs.ActivatePanel(_externalAuthPanel);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failure occurred when attempting to handle external login redirect, likely invalid data provided");
            Snackbar.Add("Invalid external login data provided, please try logging in again", Severity.Error);
        }
    }

    private async Task ForceLogin()
    {
        var result = await AccountService.ForceUserLogin(CurrentUser.Id, CurrentUser.Id);
        if (!result.Succeeded)
        {
            result.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
        }
        else
        {
            result.Messages.ForEach(x => Snackbar.Add(x, Severity.Success));
            Snackbar.Add("Successfully logged out all device sessions!");
            await Task.Delay(2500);
            await AccountService.LogoutGuiAsync(CurrentUser.Id);
            NavManager.NavigateTo(AppConfig.Value.GetLoginRedirect(LoginRedirectReason.ReAuthenticationForceUser), true);
        }
    }
}
