using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.Integrations;
using Application.Models.Identity.User;
using Application.Requests.Identity.User;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using Application.Settings.AppSettings;
using Domain.Enums.Identity;
using Domain.Enums.Integrations;
using GameWeaver.Components.Identity;
using Microsoft.Extensions.Options;

namespace GameWeaver.Pages.Identity;

public partial class Login
{
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [Parameter] public string RedirectReason { get; set; } = "";
    [Parameter] public string OauthCode { get; set; } = "";
    [Parameter] public string OauthState { get; set; } = "";

    [Inject] private IRunningServerState ServerState { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IOptions<AppConfiguration> AppSettings { get; init; } = null!;
    [Inject] private IExternalAuthProviderService ExternalAuth { get; init; } = null!;
    [Inject] private IOptions<LifecycleConfiguration> LifecycleSettings { get; init; } = null!;
    [Inject] private IOptions<OauthConfiguration> OauthSettings { get; init; } = null!;

    private string Username { get; set; } = "";
    private string Password { get; set; } = "";


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ParseParametersFromUri();
            HandleRedirectReasons();
            await HandleExternalLoginRedirect();
            StateHasChanged();
        }
    }

    private void ParseParametersFromUri()
    {
        var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
        var queryParameters = QueryHelpers.ParseQuery(uri.Query);

        if (queryParameters.TryGetValue(LoginRedirectConstants.RedirectParameter, out var redirectReason))
        {
            RedirectReason = redirectReason!;
        }

        if (queryParameters.TryGetValue(LoginRedirectConstants.OauthCode, out var oauthCode))
        {
            OauthCode = oauthCode!;
        }

        if (queryParameters.TryGetValue(LoginRedirectConstants.OauthState, out var oauthState))
        {
            OauthState = oauthState!;
        }
    }

    private void HandleRedirectReasons()
    {
        if (string.IsNullOrWhiteSpace(RedirectReason))
        {
            return;
        }

        switch (RedirectReason)
        {
            case nameof(LoginRedirectReason.SessionExpired):
                Snackbar.Add(LoginRedirectConstants.SessionExpired, Severity.Error);
                break;
            case nameof(LoginRedirectReason.ReAuthenticationForce):
                Snackbar.Add(LoginRedirectConstants.ReAuthenticationForce, Severity.Error);
                break;
            case nameof(LoginRedirectReason.ReAuthenticationForceUser):
                Snackbar.Add(LoginRedirectConstants.ReAuthenticationForceUser, Severity.Error);
                break;
            case nameof(LoginRedirectReason.FullLoginTimeout):
                Snackbar.Add(LoginRedirectConstants.FullLoginTimeout, Severity.Error);
                break;
            case nameof(LoginRedirectReason.LockedOut):
                Snackbar.Add(LoginRedirectConstants.LockedOut, Severity.Error);
                break;
            case nameof(LoginRedirectReason.Disabled):
                Snackbar.Add(LoginRedirectConstants.Disabled, Severity.Error);
                break;
            default:
                Snackbar.Add(LoginRedirectConstants.Unknown, Severity.Error);
                break;
        }
    }

    private async Task LoginAsync()
    {
        try
        {
            if (!IsRequiredInformationPresent())
            {
                return;
            }

            var loginReady = await IsMfaHandled();
            if (!loginReady)
            {
                return;
            }

            var authResponse = await AccountService.LoginGuiAsync(new UserLoginRequest
            {
                Username = Username,
                Password = Password
            });

            if (!authResponse.Succeeded)
            {
                authResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            Snackbar.Add("You're logged in, welcome to the party!", Severity.Success);

            NavManager.NavigateTo(AppSettings.Value.BaseUrl, true);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failure Occurred: {ex.Message}", Severity.Error);
        }
    }

    private async Task ForgotPassword()
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};

        await DialogService.ShowAsync<ForgotPasswordDialog>("Forgot Password", dialogOptions);
    }

    private bool IsRequiredInformationPresent()
    {
        var informationValid = true;

        if (string.IsNullOrWhiteSpace(Username))
        {
            Snackbar.Add("Username field is empty", Severity.Error);
            informationValid = false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            Snackbar.Add("Password field is empty", Severity.Error);
            informationValid = false;
        }

        return informationValid;
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

    private void RegisterAsync()
    {
        try
        {
            NavManager.NavigateTo(AppRouteConstants.Identity.Register);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failure Occurred: {ex.Message}", Severity.Error);
        }
    }

    private void DebugFillAdminCredentials()
    {
        Username = UserConstants.DefaultUsers.AdminUsername;
        Password = UserConstants.DefaultUsers.AdminPassword;
    }

    private void DebugFillModeratorCredentials()
    {
        Username = UserConstants.DefaultUsers.ModeratorUsername;
        Password = UserConstants.DefaultUsers.ModeratorPassword;
    }

    private void DebugFillBasicCredentials()
    {
        Username = UserConstants.DefaultUsers.BasicUsername;
        Password = UserConstants.DefaultUsers.BasicPassword;
    }

    private async Task<bool> IsMfaHandled()
    {
        var foundUser = await UserService.GetByUsernameSecurityFullAsync(Username);
        if (!foundUser.Succeeded || foundUser.Data is null)
        {
            Snackbar.Add(ErrorMessageConstants.Authentication.CredentialsInvalidError, Severity.Error);
            return false;
        }

        if (!foundUser.Data.TwoFactorEnabled) return true;

        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Medium, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
            {"VerifyCodeMessage", "Please enter your MFA code to login"},
            {"MfaKey", foundUser.Data.TwoFactorKey}
        };

        var mfaResponse = await DialogService.ShowAsync<MfaCodeValidationDialog>("MFA Token Validation", dialogParameters, dialogOptions);
        var dialogResult = await mfaResponse.Result;
        if (dialogResult?.Data is null)
        {
            return false;
        }

        return !dialogResult.Canceled;
    }

    private async Task InitiateExternalLogin(ExternalAuthProvider provider)
    {
        var providerLoginUriRequest = await ExternalAuth.GetLoginUri(provider, ExternalAuthRedirect.Login);
        if (!providerLoginUriRequest.Succeeded || string.IsNullOrWhiteSpace(providerLoginUriRequest.Data))
        {
            Snackbar.Add($"Failed to initiate login to desired provider: [{provider.ToString()}]", Severity.Error);
            providerLoginUriRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        NavManager.NavigateTo(providerLoginUriRequest.Data);
    }

    private async Task HandleExternalLoginRedirect()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OauthCode) || string.IsNullOrWhiteSpace(OauthState)) return;

            var redirectState = ExternalAuthHelpers.GetStateFromRedirect(OauthState);
            if (redirectState.provider == ExternalAuthProvider.Unknown)
            {
                Snackbar.Add($"Provided provider redirect is not valid: {OauthState}", Severity.Error);
                return;
            }

            var externalProfileRequest = await ExternalAuth.GetUserProfile(redirectState.provider, OauthCode);
            if (!externalProfileRequest.Succeeded)
            {
                Snackbar.Add("Provided Oauth code is invalid", Severity.Error);
                return;
            }

            if (redirectState.redirect == ExternalAuthRedirect.Security)
            {
                var securityUriCode =
                    QueryHelpers.AddQueryString(AppRouteConstants.Account.Security, LoginRedirectConstants.OauthCode, OauthCode);
                var securityUriState =
                    QueryHelpers.AddQueryString(securityUriCode, LoginRedirectConstants.OauthState, OauthState);
                var securityUriFull =
                    QueryHelpers.AddQueryString(securityUriState, LoginRedirectConstants.OauthExternalId, externalProfileRequest.Data.Id);

                NavManager.NavigateTo(securityUriFull);
                return;
            }

            var authResponse = await AccountService.LoginExternalAuthAsync(new UserExternalAuthLoginRequest
            {
                Provider = redirectState.provider,
                Email = externalProfileRequest.Data.Email,
                ExternalId = externalProfileRequest.Data.Id
            });
            if (!authResponse.Succeeded)
            {
                authResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            var authenticatedUserId = JwtHelpers.GetJwtUserId(authResponse.Data.Token);
            var userSecurityRequest = await UserService.GetByIdSecurityFullAsync(authenticatedUserId);
            if (!userSecurityRequest.Succeeded || userSecurityRequest.Data is null)
            {
                Snackbar.Add(ErrorMessageConstants.Authentication.CredentialsInvalidError, Severity.Error);
                return;
            }

            Username = userSecurityRequest.Data.Username;
            var mfaIsHandled = await IsMfaHandled();
            if (!mfaIsHandled)
            {
                return;
            }

            // If we don't have a profile avatar yet, we can check if the Oauth provider has one and use theirs instead
            var authenticatedUser = await UserService.GetByIdAsync(authenticatedUserId);
            if (string.IsNullOrWhiteSpace(authenticatedUser.Data?.ProfilePictureDataUrl) && !string.IsNullOrWhiteSpace(externalProfileRequest.Data.AvatarUri))
            {
                var updateAvatarResponse =
                    await UserService.UpdateAsync(new AppUserUpdate {Id = authenticatedUserId, ProfilePictureDataUrl = externalProfileRequest.Data.AvatarUri}, authenticatedUserId);
                if (!updateAvatarResponse.Succeeded)
                {
                    updateAvatarResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                }
            }

            Snackbar.Add("You're logged in, welcome to the party!", Severity.Success);
            NavManager.NavigateTo(AppSettings.Value.BaseUrl, true);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failure occurred when attempting to handle external login redirect, likely invalid data provided");
            Snackbar.Add("Invalid external login data provided, please try logging in again", Severity.Error);
        }
    }
}