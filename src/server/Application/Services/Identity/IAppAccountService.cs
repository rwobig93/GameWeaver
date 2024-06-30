using Application.Models.Identity.UserExtensions;
using Application.Requests.Api;
using Application.Requests.Identity.User;
using Application.Responses.v1.Api;
using Application.Responses.v1.Identity;
using Domain.Contracts;
using Domain.Enums.Identity;
using Domain.Enums.Integrations;
using Domain.Models.Identity;

namespace Application.Services.Identity;

public interface IAppAccountService
{
    Task<IResult<LocalStorageRequest>> GetLocalStorage();
    Task<IResult<UserLoginResponse>> LoginExternalAuthAsync(UserExternalAuthLoginRequest loginRequest);
    Task<IResult<UserLoginResponse>> LoginGuiAsync(UserLoginRequest loginRequest);
    Task<IResult<ApiTokenResponse>> GetApiAuthToken(ApiGetTokenRequest tokenRequest);
    Task<IResult> CacheTokensAndAuthAsync(UserLoginResponse loginResponse);
    Task<IResult> LogoutGuiAsync(Guid userId);
    Task<IResult<UserLoginResponse>> ReAuthUsingRefreshTokenAsync();
    Task<IResult<bool>> PasswordMeetsRequirements(string password);
    Task<IResult> RegisterAsync(UserRegisterRequest registerRequest);
    Task<IResult<string>> GetEmailConfirmationUrl(Guid userId, string emailAddress);
    Task<IResult<string>> ConfirmEmailAsync(Guid userId, string confirmationCode);
    Task<IResult> InitiateEmailChange(Guid userId, string newEmail);
    Task<IResult> SetUserPassword(Guid userId, string newPassword);
    Task<IResult<bool>> IsPasswordCorrect(Guid userId, string password);
    Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest forgotRequest);
    Task<IResult> ForgotPasswordConfirmationAsync(Guid userId, string confirmationCode, string password, string confirmPassword);
    Task<IResult> UpdatePreferences(Guid userId, AppUserPreferenceUpdate preferenceUpdate);
    Task<IResult<AppUserPreferenceFull>> GetPreferences(Guid userId);
    Task<IResult> ForceUserLogin(Guid userId, Guid requestUserId);
    Task<IResult> ForceUserPasswordReset(Guid userId, Guid requestUserId);
    Task<IResult> SetTwoFactorEnabled(Guid userId, bool enabled);
    Task<IResult> SetTwoFactorKey(Guid userId, string key);
    Task<IResult<bool>> DoesCurrentSessionNeedReAuthenticated();
    Task<IResult<bool>> IsRequiredToDoFullReAuthentication(Guid userId);
    Task<IResult<AuthState>> GetCurrentAuthState(Guid userId);
    Task<IResult> SetAuthState(Guid userId, AuthState authState);
    Task<IResult> GenerateUserApiToken(Guid userId, UserApiTokenTimeframe timeframe, string description);
    Task<IResult> DeleteUserApiToken(Guid userId, string value);
    Task<IResult> DeleteAllUserApiTokens(Guid userId);
    Task<IResult> SetExternalAuthProvider(Guid userId, ExternalAuthProvider provider, string externalId);
    Task<IResult> RemoveExternalAuthProvider(Guid userId, ExternalAuthProvider provider);
}