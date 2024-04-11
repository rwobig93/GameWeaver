using System.Globalization;
using System.Security.Claims;
using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Auth;
using Application.Helpers.Identity;
using Application.Helpers.Lifecycle;
using Application.Helpers.Web;
using Application.Mappers.Identity;
using Application.Models.Identity.UserExtensions;
using Application.Repositories.Identity;
using Application.Requests.v1.Api;
using Application.Requests.v1.Identity.User;
using Application.Responses.v1.Api;
using Application.Responses.v1.Identity;
using Application.Services.Identity;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Blazored.LocalStorage;
using Domain.Contracts;
using Domain.DatabaseEntities.Identity;
using Domain.Enums.Database;
using Domain.Enums.Identity;
using Domain.Enums.Integration;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;
using Domain.Models.Identity;
using Hangfire;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using IResult = Domain.Contracts.IResult;

namespace Infrastructure.Services.Identity;

public class AppAccountService : IAppAccountService 
{
    private readonly IAppUserRepository _userRepository;
    private readonly IEmailService _mailService;
    private readonly IAppRoleRepository _roleRepository;
    private readonly IAppPermissionRepository _appPermissionRepository;
    private readonly AppConfiguration _appConfig;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthStateProvider _authProvider;
    private readonly IDateTimeService _dateTime;
    private readonly IRunningServerState _serverState;
    private readonly IAuditTrailService _auditService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly SecurityConfiguration _securityConfig;
    private readonly ICurrentUserService _currentUserService;
    private readonly LifecycleConfiguration _lifecycleConfig;

    public AppAccountService(IOptions<AppConfiguration> appConfig, IAppPermissionRepository appPermissionRepository, IAppRoleRepository 
    roleRepository,
        IAppUserRepository userRepository, ILocalStorageService localStorage, AuthStateProvider authProvider,
        IEmailService mailService, IDateTimeService dateTime, IRunningServerState serverState,
        IAuditTrailService auditService, IHttpContextAccessor contextAccessor, IOptions<SecurityConfiguration> securityConfig,
        ICurrentUserService currentUserService, IOptions<LifecycleConfiguration> lifecycleConfig)
    {
        _appConfig = appConfig.Value;
        _appPermissionRepository = appPermissionRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _localStorage = localStorage;
        _authProvider = authProvider;
        _mailService = mailService;
        _dateTime = dateTime;
        _serverState = serverState;
        _auditService = auditService;
        _contextAccessor = contextAccessor;
        _currentUserService = currentUserService;
        _lifecycleConfig = lifecycleConfig.Value;
        _securityConfig = securityConfig.Value;
    }

    private static async Task<IResult<UserLoginResponse>> VerifyAccountIsLoginReady(AppUserSecurityDb userSecurity)
    {
        // Email isn't confirmed so we don't allow login
        if (!userSecurity.EmailConfirmed)
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Users.EmailNotConfirmedError);

        return userSecurity.AuthState switch
        {
            AuthState.Disabled => await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Users.AccountDisabledError),
            AuthState.LockedOut => await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Users.AccountLockedOutError),
            _ => await Result<UserLoginResponse>.SuccessAsync()
        };
    }

    private async Task<IResult<UserLoginResponse>> VerifyPasswordIsCorrect(AppUserSecurityDb userSecurity, string password)
    {
        var passwordValid = await IsPasswordCorrect(userSecurity.Id, password);
        
        // Password is valid, return success
        if (passwordValid.Data) return await Result<UserLoginResponse>.SuccessAsync();
        
        // Password provided is invalid, handle bad password
        userSecurity.BadPasswordAttempts += 1;
        userSecurity.LastBadPassword = _dateTime.NowDatabaseTime;
        await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
            
        // Account isn't locked out yet but a bad password was entered
        if (userSecurity.BadPasswordAttempts < _securityConfig.MaxBadPasswordAttempts)
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);
            
        // Account is now locked out due to bad password attempts
        userSecurity.AuthState = AuthState.LockedOut;
        userSecurity.AuthStateTimestamp = _dateTime.NowDatabaseTime;
        await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
        return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Users.AccountLockedOutError);
    }

    private async Task<IResult<UserLoginResponse>> LoginInteractiveAsync(UserLoginRequest loginRequest)
    {
        var userSecurity = (await _userRepository.GetByUsernameSecurityAsync(loginRequest.Username)).Result;
        if (userSecurity is null)
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);

        if (userSecurity.AccountType != AccountType.User)
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Users.UserAccountOnly);

        var accountIsLoginReady = await VerifyAccountIsLoginReady(userSecurity);
        if (!accountIsLoginReady.Succeeded)
            return accountIsLoginReady;

        var passwordIsValid = await VerifyPasswordIsCorrect(userSecurity, loginRequest.Password);
        if (!passwordIsValid.Succeeded)
            return passwordIsValid;
        
        // Entered password is correct so we reset previous bad password attempts and indicate full login timestamp
        userSecurity.BadPasswordAttempts = 0;
        userSecurity.LastFullLogin = _dateTime.NowDatabaseTime;
        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
        if (!updateSecurity.Succeeded)
            return await Result<UserLoginResponse>.FailAsync(updateSecurity.ErrorMessage);
        
        // Get previous client id or generate a new client id for successful login with user+pass, only registered client id's can re-auth w/ refresh tokens
        var localStorage = await GetLocalStorage();
        var clientId = localStorage.Data.ClientId ?? AccountHelpers.GenerateClientId();

        var existingClientIdRequest = await _userRepository.GetUserExtendedAttributesByTypeAndValueAsync(
            userSecurity.Id, ExtendedAttributeType.UserClientId, clientId);
        if (!existingClientIdRequest.Succeeded || existingClientIdRequest.Result?.FirstOrDefault() is null)
        {
            var newExtendedAttribute = new AppUserExtendedAttributeCreate
            {
                OwnerId = userSecurity.Id,
                Name = "FullLoginClientId",
                Type = ExtendedAttributeType.UserClientId,
                Value = clientId,
                Description = UserClientIdState.Active.ToString()
            };
            var addAttributeRequest = await _userRepository.AddExtendedAttributeAsync(newExtendedAttribute);
            if (!addAttributeRequest.Succeeded)
                return await Result<UserLoginResponse>.FailAsync(addAttributeRequest.ErrorMessage);
        }
        else
        {
            // ClientId already exists, update the description for current state
            var existingClientId = existingClientIdRequest.Result.FirstOrDefault();
            await _userRepository.UpdateExtendedAttributeAsync(existingClientId!.Id, existingClientId.Value, UserClientIdState.Active.ToString());
        }

        // Generate the JWT and return
        var token = await GenerateJwtAsync(userSecurity.ToUserDb());
        var refreshToken = JwtHelpers.GenerateUserJwtRefreshToken(_dateTime, _securityConfig, _appConfig, userSecurity.Id);
        var refreshTokenExpiration = JwtHelpers.GetJwtExpirationTime(refreshToken);
        var response = new UserLoginResponse() { ClientId = clientId, Token = token, RefreshToken = refreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiration };

        // Create audit log for login if configured
        if (_lifecycleConfig.AuditLoginLogout)
        {
            await _auditService.CreateAuditTrail(_dateTime, AuditTableName.AuthState, userSecurity.Id,
                _serverState.SystemUserId, DatabaseActionType.Login,
                new Dictionary<string, string>() {{"Username", ""}, {"AuthState", ""}},
                new Dictionary<string, string>() 
                    {{"Username", userSecurity.Username}, {"AuthState", userSecurity.AuthState.ToString()}});
        }
        
        return await Result<UserLoginResponse>.SuccessAsync(response);
    }

    private async Task<IResult<AppUserExtendedAttributeSlim>> VerifyExternalAuthIsValid(ExternalAuthProvider provider, string externalId)
    {
        var existingProviderRequest = 
            await _userRepository.GetExtendedAttributeByTypeAndValueAsync(ExtendedAttributeType.ExternalAuthLogin, externalId);
        if (!existingProviderRequest.Succeeded || existingProviderRequest.Result is null || !existingProviderRequest.Result.Any())
            return await Result<AppUserExtendedAttributeSlim>.FailAsync(ErrorMessageConstants.Authentication.ExternalAuthNotLinked);

        var matchingAuthBinding = existingProviderRequest.Result.First();
        if (matchingAuthBinding!.Description != provider.ToString())
            return await Result<AppUserExtendedAttributeSlim>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);
        
        return await Result<AppUserExtendedAttributeSlim>.SuccessAsync(matchingAuthBinding.ToSlim());
    }

    public async Task<IResult<LocalStorageRequest>> GetLocalStorage()
    {
        var tokenRequest = new LocalStorageRequest();
        
        try
        {
            tokenRequest.ClientId = await _localStorage.GetItemAsync<string>(LocalStorageConstants.ClientId);
            tokenRequest.Token = await _localStorage.GetItemAsync<string>(LocalStorageConstants.AuthToken);
            tokenRequest.RefreshToken = await _localStorage.GetItemAsync<string>(LocalStorageConstants.AuthTokenRefresh);
        }
        catch
        {
            tokenRequest.ClientId = null;
            tokenRequest.Token = "";
            tokenRequest.RefreshToken = "";
            return await Result<LocalStorageRequest>.FailAsync(tokenRequest, "Failed to load cookie authentication");
        }

        return await Result<LocalStorageRequest>.SuccessAsync(tokenRequest);
    }

    public async Task<IResult<UserLoginResponse>> LoginExternalAuthAsync(UserExternalAuthLoginRequest loginRequest)
    {
        var externalAuthIsValid = await VerifyExternalAuthIsValid(loginRequest.Provider, loginRequest.ExternalId);
        if (!externalAuthIsValid.Succeeded)
            return await Result<UserLoginResponse>.FailAsync(externalAuthIsValid.Messages);
        
        var userSecurity = (await _userRepository.GetByIdSecurityAsync(externalAuthIsValid.Data.OwnerId)).Result;
        if (userSecurity is null)
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);

        var accountIsLoginReady = await VerifyAccountIsLoginReady(userSecurity);
        if (!accountIsLoginReady.Succeeded)
            return accountIsLoginReady;
        
        // External auth is successful so we reset previous bad password attempts and indicate full login timestamp
        userSecurity.BadPasswordAttempts = 0;
        userSecurity.LastFullLogin = _dateTime.NowDatabaseTime;
        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
        if (!updateSecurity.Succeeded)
            return await Result<UserLoginResponse>.FailAsync(updateSecurity.ErrorMessage);
        
        // Generate and register client id as a successful login with user+pass, only registered client id's can re-auth w/ refresh tokens
        var clientId = AccountHelpers.GenerateClientId();
        var newExtendedAttribute = new AppUserExtendedAttributeCreate
        {
            OwnerId = userSecurity.Id,
            Name = "FullLoginClientId",
            Type = ExtendedAttributeType.UserClientId,
            Value = clientId,
            Description = ""
        };
        var addAttributeRequest = await _userRepository.AddExtendedAttributeAsync(newExtendedAttribute);
        if (!addAttributeRequest.Succeeded)
            return await Result<UserLoginResponse>.FailAsync(addAttributeRequest.ErrorMessage);

        // Generate the JWT and return
        var token = await GenerateJwtAsync(userSecurity.ToUserDb());
        var refreshToken = JwtHelpers.GenerateUserJwtRefreshToken(_dateTime, _securityConfig, _appConfig, userSecurity.Id);
        var refreshTokenExpiration = JwtHelpers.GetJwtExpirationTime(refreshToken);
        var response = new UserLoginResponse() { ClientId = clientId, Token = token, RefreshToken = refreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiration };
            
        var result = await CacheTokensAndAuthAsync(response);
        if (!result.Succeeded)
            return await Result<UserLoginResponse>.FailAsync(result.Messages.FirstOrDefault()!);

        // Create audit log for login if configured
        if (_lifecycleConfig.AuditLoginLogout)
        {
            await _auditService.CreateAuditTrail(_dateTime, AuditTableName.AuthState, userSecurity.Id,
                _serverState.SystemUserId, DatabaseActionType.Login,
                new Dictionary<string, string>() {{"Username", ""}, {"AuthState", ""}},
                new Dictionary<string, string>() 
                    {{"Username", userSecurity.Username}, {"AuthState", userSecurity.AuthState.ToString()}});
        }
        
        return await Result<UserLoginResponse>.SuccessAsync(response);
    }

    public async Task<IResult<UserLoginResponse>> LoginGuiAsync(UserLoginRequest loginRequest)
    {
        try
        {
            var loginResponse = await LoginInteractiveAsync(loginRequest);
            if (!loginResponse.Succeeded)
                return loginResponse;
            
            var result = await CacheTokensAndAuthAsync(loginResponse.Data);
            if (!result.Succeeded)
                return await Result<UserLoginResponse>.FailAsync(result.Messages.FirstOrDefault()!);

            return await Result<UserLoginResponse>.SuccessAsync(loginResponse.Data);
        }
        catch (Exception ex)
        {
            return await Result<UserLoginResponse>.FailAsync($"Failure occurred attempting to login: {ex.Message}");
        }
    }

    public async Task<IResult<ApiTokenResponse>> GetApiAuthToken(ApiGetTokenRequest tokenRequest)
    {
        var userSecurity = (await _userRepository.GetByUsernameSecurityAsync(tokenRequest.Username)).Result;
        if (userSecurity is null)
            return await Result<ApiTokenResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);
        
        return userSecurity.AuthState switch
        {
            AuthState.Disabled => await Result<ApiTokenResponse>.FailAsync(ErrorMessageConstants.Users.AccountDisabledError),
            AuthState.LockedOut => await Result<ApiTokenResponse>.FailAsync(ErrorMessageConstants.Users.AccountLockedOutError),
            _ => userSecurity.AccountType switch
            {
                AccountType.User => await HandleApiAuthUserAccount(tokenRequest, userSecurity),
                AccountType.Service => await HandleApiAuthServiceAccount(tokenRequest, userSecurity),
                _ => await Result<ApiTokenResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError)
            }
        };
    }

    private async Task<IResult<ApiTokenResponse>> HandleApiAuthUserAccount(ApiGetTokenRequest tokenRequest, AppUserSecurityDb userSecurity)
    {
        var userApiTokensRequest = await _userRepository.GetUserExtendedAttributesByTypeAndValueAsync(
            userSecurity.Id, ExtendedAttributeType.UserApiToken, tokenRequest.Password);
        if (!userApiTokensRequest.Succeeded || userApiTokensRequest.Result is null || !userApiTokensRequest.Result.Any())
        {
            userSecurity.BadPasswordAttempts += 1;
            userSecurity.LastBadPassword = _dateTime.NowDatabaseTime;
            await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());

            // Account isn't locked out yet but a bad api token was provided
            if (userSecurity.BadPasswordAttempts < _securityConfig.MaxBadPasswordAttempts)
                return await Result<ApiTokenResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);

            // Account is now locked out due to bad api tokens being provided
            userSecurity.AuthState = AuthState.LockedOut;
            userSecurity.AuthStateTimestamp = _dateTime.NowDatabaseTime;
            await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
            return await Result<ApiTokenResponse>.FailAsync(ErrorMessageConstants.Users.AccountLockedOutError);
        }

        // Provided api token is correct so we reset previous bad password attempts
        userSecurity.BadPasswordAttempts = 0;

        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
        if (!updateSecurity.Succeeded)
            return await Result<ApiTokenResponse>.FailAsync(updateSecurity.ErrorMessage);

        var token = await GenerateJwtAsync(userSecurity.ToUserDb(), true);
        var response = new ApiTokenResponse() {Token = token, TokenExpiration = JwtHelpers.GetJwtExpirationTime(token)};

        return await Result<ApiTokenResponse>.SuccessAsync(response);
    }

    private async Task<IResult<ApiTokenResponse>> HandleApiAuthServiceAccount(ApiGetTokenRequest tokenRequest, AppUserSecurityDb userSecurity)
    {
        var passwordValid = await IsPasswordCorrect(userSecurity.Id, tokenRequest.Password);
        if (!passwordValid.Data)
        {
            userSecurity.BadPasswordAttempts += 1;
            userSecurity.LastBadPassword = _dateTime.NowDatabaseTime;
            await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());

            // Account isn't locked out yet but a bad password was entered
            if (userSecurity.BadPasswordAttempts < _securityConfig.MaxBadPasswordAttempts)
                return await Result<ApiTokenResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);

            // Account is now locked out due to bad password attempts
            userSecurity.AuthState = AuthState.LockedOut;
            userSecurity.AuthStateTimestamp = _dateTime.NowDatabaseTime;
            await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
            return await Result<ApiTokenResponse>.FailAsync(ErrorMessageConstants.Users.AccountLockedOutError);
        }

        // Entered password is correct so we reset previous bad password attempts
        userSecurity.BadPasswordAttempts = 0;

        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
        if (!updateSecurity.Succeeded)
            return await Result<ApiTokenResponse>.FailAsync(updateSecurity.ErrorMessage);

        var token = await GenerateJwtAsync(userSecurity.ToUserDb(), true);
        var response = new ApiTokenResponse() {Token = token, TokenExpiration = JwtHelpers.GetJwtExpirationTime(token)};

        return await Result<ApiTokenResponse>.SuccessAsync(response);
    }

    public async Task<IResult> CacheTokensAndAuthAsync(UserLoginResponse loginResponse)
    {
        try
        {
            await _localStorage.SetItemAsync(LocalStorageConstants.ClientId, loginResponse.ClientId);
            await _localStorage.SetItemAsync(LocalStorageConstants.AuthToken, loginResponse.Token);
            await _localStorage.SetItemAsync(LocalStorageConstants.AuthTokenRefresh, loginResponse.RefreshToken);

            var authState = await _authProvider.GetAuthenticationStateAsync(loginResponse.Token);
            if (authState.User.Identity?.Name == UserConstants.UnauthenticatedIdentity.Name)
                return await Result.FailAsync(ErrorMessageConstants.Authentication.TokenInvalidError);
            
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> LogoutGuiAsync(Guid userId)
    {
        try
        {
            // Remove client id and tokens from local client storage and de-authenticate
            await _localStorage.RemoveItemAsync(LocalStorageConstants.AuthToken);
            await _localStorage.RemoveItemAsync(LocalStorageConstants.AuthTokenRefresh);
            _authProvider.DeAuthenticateUser();
            
            if (!_lifecycleConfig.AuditLoginLogout) return await Result.SuccessAsync();
            
            // Create audit log for logout if configured
            if (userId == Guid.Empty)
                userId = await _currentUserService.GetCurrentUserId() ?? Guid.Empty;
                
            var user = (await _userRepository.GetByIdAsync(userId)).Result ??
                       new AppUserSecurityDb() {Username = "Unknown", Email = "User@Unknown.void"};
            
            await _auditService.CreateAuditTrail(_dateTime, AuditTableName.AuthState, userId,
                _serverState.SystemUserId, DatabaseActionType.Logout,
                new Dictionary<string, string>()
                    {{"Username", user.Username}, {"AuthState", user.AuthState.ToString()}},
                new Dictionary<string, string>() {{"Username", ""}, {"AuthState", ""}});

            return await Result.SuccessAsync();
        }
        catch
        {
            return await Result.FailAsync();
        }
    }

    public async Task<IResult<UserLoginResponse>> ReAuthUsingRefreshTokenAsync()
    {
        // Get tokens and clientId from local storage
        var localStorageRequest = await GetLocalStorage();
        if (!localStorageRequest.Succeeded)
            return await Result<UserLoginResponse>.FailAsync(localStorageRequest.Messages);

        var localStorage = localStorageRequest.Data;
        
        if (string.IsNullOrWhiteSpace(localStorage.ClientId) ||
            string.IsNullOrWhiteSpace(localStorage.Token) ||
            string.IsNullOrWhiteSpace(localStorage.RefreshToken))
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Authentication.TokenInvalidError);
        
        var tokenUserId = JwtHelpers.GetJwtUserId(localStorage.Token);
        var refreshTokenUserId = JwtHelpers.GetJwtUserId(localStorage.RefreshToken);
        
        // Validate provided token and refresh token user ID's match, otherwise the request is suspicious
        if (tokenUserId != refreshTokenUserId)
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Authentication.TokenInvalidError);
        
        var user = (await _userRepository.GetByIdAsync(refreshTokenUserId)).Result;
        if (user == null)
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Authentication.TokenInvalidError);
        
        // Validate the provided client id has been registered, if not this client can't use a refresh token
        var clientIdRequest = await _userRepository.GetUserExtendedAttributesByTypeAndValueAsync(
            user.Id, ExtendedAttributeType.UserClientId, localStorage.ClientId);
        if (!clientIdRequest.Succeeded || clientIdRequest.Result is null || !clientIdRequest.Result.Any())
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Authentication.TokenInvalidError);
        
        if (!JwtHelpers.IsJwtValid(localStorage.RefreshToken, _securityConfig, _appConfig))
            return await Result<UserLoginResponse>.FailAsync(ErrorMessageConstants.Authentication.TokenInvalidError);
        
        // Token & Refresh Token have been validated and matched, now re-auth the user and generate new tokens
        var token = JwtHelpers.GenerateUserJwtEncryptedToken(await GetClaimsAsync(user.ToUserDb()), _dateTime, _securityConfig, _appConfig);
        var refreshToken = JwtHelpers.GenerateUserJwtRefreshToken(_dateTime, _securityConfig, _appConfig, user.Id);
        var refreshTokenExpiration = JwtHelpers.GetJwtExpirationTime(refreshToken);
        var clientId = clientIdRequest.Result.FirstOrDefault();
        await _userRepository.UpdateExtendedAttributeAsync(clientId!.Id, clientId.Value, UserClientIdState.Active.ToString());

        var response = new UserLoginResponse { ClientId = localStorage.ClientId, Token = token, RefreshToken = refreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiration };

        // Cache new tokens and authenticate user principal
        var cacheRequest = await CacheTokensAndAuthAsync(response);
        if (!cacheRequest.Succeeded)
            return await Result<UserLoginResponse>.FailAsync(cacheRequest.Messages);

        return await Result<UserLoginResponse>.SuccessAsync(response);
    }

    public async Task<IResult<bool>> PasswordMeetsRequirements(string password)
    {
        try
        {
            return await Result<bool>.SuccessAsync(AccountHelpers.DoesPasswordMeetRequirements(password));
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    private async Task<DatabaseActionResult<Guid>> CreateAsync(AppUserDb user, string password)
    {
        var createUser = user.ToCreateObject();
        var passwordMeetsRequirements = (await PasswordMeetsRequirements(password)).Data;
        if (!passwordMeetsRequirements)
        {
            var passwordFailResult = new DatabaseActionResult<Guid>();
            passwordFailResult.Fail("Provided password doesn't meet the requirements");
            return passwordFailResult;
        }

        AccountHelpers.GenerateHashAndSalt(password, _securityConfig.PasswordPepper, out var salt, out var hash);
        createUser.CreatedOn = _dateTime.NowDatabaseTime;
        createUser.CreatedBy = _serverState.SystemUserId;

        var userSecurity = new AppUserSecurityAttributeCreate
        {
            PasswordSalt = salt,
            PasswordHash = hash
        };
        await _userRepository.CreateSecurityAsync(userSecurity);

        return await _userRepository.CreateAsync(createUser);
    }

    public async Task<IResult> RegisterAsync(UserRegisterRequest registerRequest)
    {
        if (!AccountHelpers.IsValidEmailAddress(registerRequest.Email))
            return await Result.FailAsync($"The email address {registerRequest.Email} provided isn't a valid email, please try again");
        
        var matchingEmail = (await _userRepository.GetByEmailAsync(registerRequest.Email)).Result;
        if (matchingEmail is not null)
            return await Result.FailAsync(
                $"The email address {registerRequest.Email} is already in use, are you sure you don't have an account already?");
        
        var matchingUserName = (await _userRepository.GetByUsernameAsync(registerRequest.Username)).Result;
        if (matchingUserName != null)
        {
            return await Result.FailAsync(string.Format($"Username {registerRequest.Username} is already in use, please try again"));
        }

        var passwordMeetsRequirements = await PasswordMeetsRequirements(registerRequest.Password);
        if (!passwordMeetsRequirements.Succeeded || !passwordMeetsRequirements.Data)
        {
            var issuesWithPassword = AccountHelpers.GetAnyIssuesWithPassword(registerRequest.Password);
            return await Result.FailAsync(issuesWithPassword);
        }
        
        var newUser = new AppUserDb()
        {
            Email = registerRequest.Email,
            Username = registerRequest.Username,
        };
        
        var createUserResult = await CreateAsync(newUser, registerRequest.Password);
        if (!createUserResult.Succeeded)
            return await Result.FailAsync(createUserResult.ErrorMessage);

        if (_securityConfig.NewlyRegisteredAccountsDisabled)
        {
            var currentSecurity = await _userRepository.GetSecurityAsync(createUserResult.Result);
            var securityUpdate = currentSecurity.Result!.ToUpdate();
            securityUpdate.AuthState = AuthState.Disabled;
            await _userRepository.UpdateSecurityAsync(securityUpdate);
        }

        var caveatMessage = "";
        var defaultRole = (await _roleRepository.GetByNameAsync(RoleConstants.DefaultRoles.DefaultName)).Result;
        var addToRoleResult = await _roleRepository.AddUserToRoleAsync(
            createUserResult.Result, defaultRole!.Id, _serverState.SystemUserId);
        if (!addToRoleResult.Succeeded)
            caveatMessage = $",{Environment.NewLine} Default permissions could not be added to this account, " +
                            $"please contact the administrator for assistance";

        var confirmationUrl = (await GetEmailConfirmationUrl(createUserResult.Result, registerRequest.Email)).Data;
        if (string.IsNullOrWhiteSpace(confirmationUrl))
            return await Result.FailAsync("Failure occurred generating confirmation URL, please contact the administrator");
        
        var response = BackgroundJob.Enqueue(() => _mailService.SendRegistrationEmail(newUser.Email, newUser.Username, confirmationUrl));

        if (response is null)
            return await Result.FailAsync(
                $"Account was registered successfully but a failure occurred attempting to send an email to " +
                $"the address provided, please contact the administrator for assistance{caveatMessage}");
        
        return await Result<Guid>.SuccessAsync(newUser.Id, 
            $"Account {newUser.Username} successfully registered, please check your email to confirm!{caveatMessage}");
    }

    public async Task<IResult<string>> GetEmailConfirmationUrl(Guid userId, string emailAddress)
    {
        var previousConfirmations =
            (await _userRepository.GetUserExtendedAttributesByTypeAsync(userId, ExtendedAttributeType.EmailConfirmationToken)).Result;
        var previousConfirmation = previousConfirmations?.FirstOrDefault();
        
        var endpointUri = new Uri(string.Concat(_appConfig.BaseUrl, AppRouteConstants.Identity.ConfirmEmail));
        var confirmationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", userId.ToString());
        
        // Previous pending account registration exists, return current value
        if (previousConfirmation is not null)
            return await Result<string>.SuccessAsync(QueryHelpers.AddQueryString(
                confirmationUri, "confirmationCode", previousConfirmation.Value));

        // No currently pending account registration exists so we'll generate a new one, add it to the provided user
        //   and return the generated confirmation uri
        var confirmationCode = UrlHelpers.GenerateToken();
        var newExtendedAttribute = new AppUserExtendedAttributeCreate
        {
            OwnerId = userId,
            Name = emailAddress,
            Type = ExtendedAttributeType.EmailConfirmationToken,
            Value = confirmationCode,
            Description = ""
        };
        var addAttributeRequest = await _userRepository.AddExtendedAttributeAsync(newExtendedAttribute);
        if (!addAttributeRequest.Succeeded)
            return await Result<string>.FailAsync(addAttributeRequest.ErrorMessage);

        return await Result<string>.SuccessAsync(QueryHelpers.AddQueryString(confirmationUri, "confirmationCode", confirmationCode));
    }

    public async Task<IResult<string>> ConfirmEmailAsync(Guid userId, string confirmationCode)
    {
        var userSecurity = (await _userRepository.GetByIdSecurityAsync(userId)).Result;
        if (userSecurity is null)
            return await Result<string>.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);

        var previousConfirmations =
            (await _userRepository.GetUserExtendedAttributesByTypeAsync(userSecurity.Id, ExtendedAttributeType.EmailConfirmationToken)).Result;
        var previousConfirmation = previousConfirmations?.FirstOrDefault();

        if (previousConfirmation is null)
        {
            await _auditService.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootConfirmation,
                userId, new Dictionary<string, string>
                {
                    {"UserId", userSecurity.Id.ToString()},
                    {"Username", userSecurity.Username},
                    {"Email", userSecurity.Email},
                    {"Details", "Email confirmation was attempted when an email confirmation isn't currently pending"}
                });
            
            return await Result<string>.FailAsync(
                $"An error occurred attempting to confirm account: {userSecurity.Id}, please contact the administrator");
        }
        
        if (previousConfirmation.Value != confirmationCode)
        {
            await _auditService.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootConfirmation,
                userSecurity.Id, new Dictionary<string, string>
                {
                    {"UserId", userSecurity.Id.ToString()},
                    {"Username", userSecurity.Username},
                    {"Email", userSecurity.Email},
                    {"Details", "Email confirmation was attempted with an invalid confirmation code"},
                    {"Correct Code ", previousConfirmation.Value},
                    {"Provided Code", confirmationCode}
                });
            
            return await Result<string>.FailAsync(
                $"An error occurred attempting to confirm account: {userSecurity.Id}, please contact the administrator");
        }

        userSecurity.Email = previousConfirmation.Name;
        if (!_securityConfig.NewlyRegisteredAccountsDisabled)
            userSecurity.AuthState = AuthState.Enabled;
        userSecurity.EmailConfirmed = true;
        userSecurity.LastModifiedBy = _serverState.SystemUserId;
        userSecurity.LastModifiedOn = _dateTime.NowDatabaseTime;

        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.ToSecurityUpdate());
        if (!updateSecurity.Succeeded)
            return await Result<string>.FailAsync(updateSecurity.ErrorMessage);

        var confirmEmail = await _userRepository.UpdateAsync(userSecurity.ToUserUpdate());
        if (!confirmEmail.Succeeded)
            return await Result<string>.FailAsync(
                $"An error occurred attempting to confirm account: {userSecurity.Id}, please contact the administrator");
        await _userRepository.RemoveExtendedAttributeAsync(previousConfirmation.Id);
        
        return await Result<string>.SuccessAsync(userSecurity.Id.ToString(), $"Email Confirmed for {userSecurity.Username}");
    }

    public async Task<IResult> InitiateEmailChange(Guid userId, string newEmail)
    {
        var foundUserRequest = await _userRepository.GetByIdAsync(userId);
        if (!foundUserRequest.Succeeded || foundUserRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);

        if (!AccountHelpers.IsValidEmailAddress(newEmail))
            return await Result.FailAsync($"The email address {newEmail} provided isn't a valid email, please try again");
        
        var matchingEmail = (await _userRepository.GetByEmailAsync(newEmail)).Result;
        if (matchingEmail is not null)
            return await Result.FailAsync(
                $"The email address {newEmail} is already in use and cannot be assigned to another account");

        var confirmationUrl = (await GetEmailConfirmationUrl(foundUserRequest.Result.Id, newEmail)).Data;
        if (string.IsNullOrWhiteSpace(confirmationUrl))
        {
            await _auditService.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootConfirmation,
                foundUserRequest.Result.Id, new Dictionary<string, string>()
                {
                    {"UserId", foundUserRequest.Result.Id.ToString()},
                    {"Username", foundUserRequest.Result.Username},
                    {"Email", foundUserRequest.Result.Email},
                    {"Details", "Was unable to generate confirmation Url"},
                    {"Confirmation Url", confirmationUrl}
                });
            return await Result.FailAsync("Failure occurred generating confirmation URL, please contact the administrator");
        }
        
        var response = BackgroundJob.Enqueue(() =>
            _mailService.SendEmailChangeConfirmation(newEmail, foundUserRequest.Result.Username, confirmationUrl));
        if (response is not null) return await Result.SuccessAsync("Email confirmation sent to the email address provided");
        
        await _auditService.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootConfirmation,
            foundUserRequest.Result.Id, new Dictionary<string, string>()
            {
                {"UserId", foundUserRequest.Result.Id.ToString()},
                {"Username", foundUserRequest.Result.Username},
                {"Email", foundUserRequest.Result.Email},
                {"Details", $"Failure occurred sending email confirmation to {newEmail}"}
            });
        return await Result.FailAsync($"Failed to send confirmation email to {newEmail}, please contact the administrator");
    }

    public async Task<IResult> SetUserPassword(Guid userId, string newPassword)
    {
        try
        {
            var userSecurityRequest = await _userRepository.GetByIdSecurityAsync(userId);
            if (!userSecurityRequest.Succeeded || userSecurityRequest.Result is null)
                return await Result.FailAsync(userSecurityRequest.ErrorMessage);
            
            var passwordMeetsRequirements = await PasswordMeetsRequirements(newPassword);
            if (!passwordMeetsRequirements.Succeeded || !passwordMeetsRequirements.Data)
            {
                var issuesWithPassword = AccountHelpers.GetAnyIssuesWithPassword(newPassword);
                return await Result.FailAsync(issuesWithPassword);
            }
            
            AccountHelpers.GenerateHashAndSalt(newPassword, _securityConfig.PasswordPepper, out var salt, out var hash);
            
            var securityUpdate = userSecurityRequest.Result.ToSecurityUpdate();
            securityUpdate.PasswordSalt = salt;
            securityUpdate.PasswordHash = hash;
            
            var securityResult = await _userRepository.UpdateSecurityAsync(securityUpdate);
            if (!securityResult.Succeeded)
                return await Result.FailAsync(securityResult.ErrorMessage);

            var userUpdate = userSecurityRequest.Result.ToUpdate();
            userUpdate.LastModifiedBy = _serverState.SystemUserId;
            userUpdate.LastModifiedOn = _dateTime.NowDatabaseTime;
            
            var userResult = await _userRepository.UpdateAsync(userUpdate);
            if (!userResult.Succeeded)
                return await Result.FailAsync(userResult.ErrorMessage);
            
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> IsPasswordCorrect(Guid userId, string password)
    {
        try
        {
            var matchingSecurity = (await _userRepository.GetSecurityAsync(userId)).Result;
            var passwordCorrect = AccountHelpers.IsPasswordCorrect(
                password, matchingSecurity!.PasswordSalt, _securityConfig.PasswordPepper, matchingSecurity.PasswordHash);
            return await Result<bool>.SuccessAsync(passwordCorrect);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest forgotRequest)
    {
        var foundUser = (await _userRepository.GetByEmailAsync(forgotRequest.Email!)).Result;
        if (foundUser is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.ContactAdmin);

        if (!foundUser.EmailConfirmed)
            return await Result.FailAsync(ErrorMessageConstants.Users.EmailNotConfirmedError);

        var previousResets =
            (await _userRepository.GetUserExtendedAttributesByTypeAsync(foundUser.Id, ExtendedAttributeType.PasswordResetToken)).Result;
        var previousReset = previousResets?.FirstOrDefault();
        
        var endpointUri = new Uri(string.Concat(_appConfig.BaseUrl, AppRouteConstants.Identity.ForgotPassword));
        var confirmationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", foundUser.Id.ToString());
        
        // Previous pending forgot password exists, return current value
        if (previousReset is not null)
        {
            confirmationUri = QueryHelpers.AddQueryString(confirmationUri, "confirmationCode", previousReset.Value);
        }
        else
        {
            // No currently pending forgot password request exists so we'll generate a new one, add it to the provided user
            //   and return the generated reset uri
            var confirmationCode = UrlHelpers.GenerateToken();
            var newExtendedAttribute = new AppUserExtendedAttributeCreate()
            {
                OwnerId = foundUser.Id,
                Name = "ForgotPasswordReset",
                Type = ExtendedAttributeType.PasswordResetToken,
                Value = confirmationCode,
                Description = ""
            };
            await _userRepository.AddExtendedAttributeAsync(newExtendedAttribute);
            confirmationUri = QueryHelpers.AddQueryString(confirmationUri, "confirmationCode", newExtendedAttribute.Value);
        }

        var response =
            BackgroundJob.Enqueue(() => _mailService.SendPasswordResetEmail(foundUser.Email, foundUser.Username, confirmationUri));
        if (response is null)
            return await Result.FailAsync(
                "Failure occurred attempting to send the password reset email, please reach out to the administrator");

        return await Result.SuccessAsync("Successfully sent password reset to the email provided!");
    }

    public async Task<IResult> ForgotPasswordConfirmationAsync(Guid userId, string confirmationCode, string password, string confirmPassword)
    {
        var foundUser = (await _userRepository.GetByIdAsync(userId)).Result;
        if (foundUser is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.ContactAdmin);

        var previousResets =
            (await _userRepository.GetUserExtendedAttributesByTypeAsync(foundUser.Id, ExtendedAttributeType.PasswordResetToken)).Result;
        var previousReset = previousResets?.FirstOrDefault();
        
        if (previousReset is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.ContactAdmin);
        if (password != confirmPassword)
            return await Result.FailAsync(ErrorMessageConstants.Authentication.PasswordsNoMatchError);
        if (confirmationCode != previousReset.Value)
            return await Result.FailAsync(ErrorMessageConstants.Authentication.TokenInvalidError);

        var passwordMeetsRequirements = (await PasswordMeetsRequirements(password)).Data;
        if (!passwordMeetsRequirements)
            return await Result.FailAsync("Password provided doesn't meet the minimum requirements, please try again");
        
        await SetUserPassword(foundUser.Id, password);

        await _userRepository.RemoveExtendedAttributeAsync(previousReset.Id);
        await SetAuthState(userId, AuthState.Enabled);

        return await Result.SuccessAsync("Password reset was successful, please log back in with your fresh new password!");
    }

    public async Task<IResult> UpdatePreferences(Guid userId, AppUserPreferenceUpdate preferenceUpdate)
    {
        var updateRequest = await _userRepository.UpdatePreferences(userId, preferenceUpdate);
        if (!updateRequest.Succeeded)
            return await Result.FailAsync($"Failure occurred attempting to update preferences: {updateRequest.ErrorMessage}");

        return await Result.SuccessAsync("Preferences updated successfully");
    }

    public async Task<IResult<AppUserPreferenceFull>> GetPreferences(Guid userId)
    {
        var preferences = await _userRepository.GetPreferences(userId);
        if (!preferences.Succeeded)
            return await Result<AppUserPreferenceFull>.FailAsync($"Failure occurred getting preferences: {preferences.ErrorMessage}");

        if (preferences.Result is null)
            return await Result<AppUserPreferenceFull>.FailAsync("Preferences couldn't be found for the UserId provided");

        var preferencesFull = preferences.Result.ToFull();
            
        preferencesFull.CustomThemeOne = JsonConvert.DeserializeObject<AppThemeCustom>(preferences.Result.CustomThemeOne!) ?? new AppThemeCustom();
        preferencesFull.CustomThemeTwo = JsonConvert.DeserializeObject<AppThemeCustom>(preferences.Result.CustomThemeTwo!) ?? new AppThemeCustom();
        preferencesFull.CustomThemeThree = JsonConvert.DeserializeObject<AppThemeCustom>(preferences.Result.CustomThemeThree!) ?? new AppThemeCustom();

        return await Result<AppUserPreferenceFull>.SuccessAsync(preferencesFull);
    }

    public async Task<IResult> ForceUserLogin(Guid userId)
    {
        var userSecurity = await _userRepository.GetByIdSecurityAsync(userId);
        if (!userSecurity.Succeeded || userSecurity.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);

        // Update account auth state to indicate login is required
        userSecurity.Result.AuthState = AuthState.LoginRequired;
        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.Result.ToSecurityUpdate());
        if (!updateSecurity.Succeeded)
            return await Result.FailAsync(updateSecurity.ErrorMessage);
        
        // Grab all registered client id's for the user account
        var userClientIdRequest =
            await _userRepository.GetUserExtendedAttributesByTypeAsync(userSecurity.Result.Id, ExtendedAttributeType.UserClientId);
        if (!userClientIdRequest.Succeeded)
            return await Result.FailAsync(userClientIdRequest.ErrorMessage);

        var messages = new List<string>();
        
        // Remove all client id's for the specified user account which will require a user to login when the primary JWT expires
        //   This also means a refresh token cannot be used for any client without doing a full user+pass authentication
        var userClientIds = (userClientIdRequest.Result ?? new List<AppUserExtendedAttributeDb>()).ToArray();
        if (userClientIds.Any())
        {
            foreach (var clientId in userClientIds)
            {
                var removeRequest = await _userRepository.RemoveExtendedAttributeAsync(clientId.Id);
                if (!removeRequest.Succeeded)
                    messages.Add(removeRequest.ErrorMessage);
            }
        }

        if (messages.Any())
            return await Result.FailAsync(messages);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> ForceUserPasswordReset(Guid userId)
    {
        var userSecurity = await _userRepository.GetByIdSecurityAsync(userId);
        if (!userSecurity.Succeeded)
            return await Result.FailAsync(userSecurity.ErrorMessage);

        var forceLoginRequest = await ForceUserLogin(userId);
        if (!forceLoginRequest.Succeeded)
            return await Result.FailAsync(forceLoginRequest.Messages);
        
        await SetUserPassword(userId, UrlHelpers.GenerateToken());
        return await ForgotPasswordAsync(new ForgotPasswordRequest() { Email = userSecurity.Result!.Email });
    }

    public async Task<IResult> SetTwoFactorEnabled(Guid userId, bool enabled)
    {
        var userSecurity = await _userRepository.GetSecurityAsync(userId);
        if (!userSecurity.Succeeded || userSecurity.Result is null)
            return await Result.FailAsync(userSecurity.ErrorMessage);

        userSecurity.Result.TwoFactorEnabled = enabled;

        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.Result.ToUpdate());
        if (!updateSecurity.Succeeded)
            return await Result.FailAsync(updateSecurity.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> SetTwoFactorKey(Guid userId, string key)
    {
        var userSecurity = await _userRepository.GetSecurityAsync(userId);
        if (!userSecurity.Succeeded || userSecurity.Result is null)
            return await Result.FailAsync(userSecurity.ErrorMessage);

        userSecurity.Result.TwoFactorKey = key;

        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.Result.ToUpdate());
        if (!updateSecurity.Succeeded)
            return await Result.FailAsync(updateSecurity.ErrorMessage);

        return await Result.SuccessAsync();
    }

    private async Task<string> GenerateJwtAsync(AppUserDb user, bool isApiToken = false)
    {
        return isApiToken ?
            JwtHelpers.GenerateApiJwtEncryptedToken(await GetClaimsAsync(user), _dateTime, _securityConfig, _appConfig) : 
            JwtHelpers.GenerateUserJwtEncryptedToken(await GetClaimsAsync(user), _dateTime, _securityConfig, _appConfig);
    }

    private async Task<IEnumerable<Claim>> GetClaimsAsync(AppUserDb user)
    {
        var allUserAndRolePermissions = 
            (await _appPermissionRepository.GetAllIncludingRolesForUserAsync(user.Id)).Result?.ToClaims() ?? new List<Claim>();
        var allRoles = 
            (await _roleRepository.GetRolesForUser(user.Id)).Result?.ToClaims() ?? new List<Claim>();

        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Username)
            }
        .Union(allUserAndRolePermissions)
        .Union(allRoles);

        return claims;
    }

    private async Task<string> GetAuthTokenFromSession()
    {
        var authToken = GetTokenFromHttpAuthorizationHeader();
        if (!string.IsNullOrWhiteSpace(authToken))
            return authToken;
        
        authToken = await GetTokenFromLocalStorage();

        return authToken;
    }

    private string GetTokenFromHttpAuthorizationHeader()
    {
        try
        {
            var headerHasValue = _contextAccessor.HttpContext!.Request.Headers.TryGetValue("Authorization", out var bearer);
            if (!headerHasValue)
                return "";
            
            // Authorization header should always be: <scheme> <token>, which in our case is: Bearer JWT
            return bearer.ToString().Split(' ')[1];
        }
        catch
        {
            return "";
        }
    }

    private async Task<string> GetTokenFromLocalStorage()
    {
        try
        {
            return await _localStorage.GetItemAsync<string>(LocalStorageConstants.AuthToken);
        }
        catch
        {
            // Since Blazor Server pre-rendering has the state received twice and we can't have JSInterop run while rendering is occurring
            //   we have to do this to keep our sanity, would love to find a working solution to this at some point
            return "";
        }
    }

    public async Task<IResult<bool>> DoesCurrentSessionNeedReAuthenticated()
    {
        try
        {
            var authToken = await GetAuthTokenFromSession();
            var sessionIsValid = JwtHelpers.IsJwtValid(authToken, _securityConfig, _appConfig);
            if (!sessionIsValid)
                return await Result<bool>.SuccessAsync(true);

            var currentUserId = JwtHelpers.GetJwtUserId(authToken);
            var localStorageRequest = await GetLocalStorage();
            if (!localStorageRequest.Succeeded)
                return await Result<bool>.FailAsync(localStorageRequest.Messages);
        
            // Validate if the current clientId needs to re-authenticate due to permission changes
            var clientIdRequest = await _userRepository.GetUserExtendedAttributesByTypeAndValueAsync(
                currentUserId, ExtendedAttributeType.UserClientId, localStorageRequest.Data.ClientId ?? "");
            if (!clientIdRequest.Succeeded || clientIdRequest.Result is null || !clientIdRequest.Result.Any())
                return await Result<bool>.FailAsync(ErrorMessageConstants.Authentication.TokenInvalidError);

            if (clientIdRequest.Result.FirstOrDefault()!.Description == UserClientIdState.ReAuthNeeded.ToString())
                return await Result<bool>.SuccessAsync(true);

            // Session doesn't need re-auth
            return await Result<bool>.SuccessAsync(false);
        }
        catch (Exception ex)
        {
            // Token isn't valid and has likely expired
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> IsUserRequiredToReAuthenticate(Guid userId)
    {
        try
        {
            var userSecurity = await _userRepository.GetSecurityAsync(userId);
            userSecurity.Result!.LastFullLogin ??= _dateTime.NowDatabaseTime;
            
            // If configured force login time has passed since last full login we want the user to login again
            if (userSecurity.Result!.LastFullLogin!.Value.AddMinutes(_securityConfig.ForceLoginIntervalMinutes) < _dateTime.NowDatabaseTime)
                return await Result<bool>.SuccessAsync(true);
            
            // If account auth state is set to force re-login we want the user to login again
            return await Result<bool>.SuccessAsync(userSecurity.Result!.AuthState == AuthState.LoginRequired);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> SetAuthState(Guid userId, AuthState authState)
    {
        var userSecurity = await _userRepository.GetSecurityAsync(userId);
        if (!userSecurity.Succeeded || userSecurity.Result is null)
            return await Result.FailAsync(userSecurity.ErrorMessage);

        userSecurity.Result.AuthState = authState;
        // If we are enabling the account we'll reset bad password attempts
        if (authState == AuthState.Enabled)
            userSecurity.Result.BadPasswordAttempts = 0;

        var updateSecurity = await _userRepository.UpdateSecurityAsync(userSecurity.Result.ToUpdate());
        if (!updateSecurity.Succeeded)
            return await Result.FailAsync(updateSecurity.ErrorMessage);

        return await Result.SuccessAsync($"User account successfully set: {userSecurity.Result.AuthState.ToString()}");
    }

    public async Task<IResult> GenerateUserApiToken(Guid userId, UserApiTokenTimeframe timeframe, string description)
    {
        var foundUserRequest = await _userRepository.GetByIdAsync(userId);
        if (!foundUserRequest.Succeeded || foundUserRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);

        var userApiToken = UrlHelpers.GenerateToken(_securityConfig.UserApiTokenSizeInBytes);
        var tokenExpiration = UserApiHelpers.GetUserApiTokenExpirationTime(_dateTime, timeframe);

        var newExtendedAttribute = new AppUserExtendedAttributeCreate()
        {
            OwnerId = foundUserRequest.Result.Id,
            Name = tokenExpiration.ToString(CultureInfo.CurrentCulture),
            Type = ExtendedAttributeType.UserApiToken,
            Value = userApiToken,
            Description = description
        };
        var addRequest = await _userRepository.AddExtendedAttributeAsync(newExtendedAttribute);
        if (!addRequest.Succeeded)
            return await Result.FailAsync(addRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteUserApiToken(Guid userId, string value)
    {
        var foundUserRequest = await _userRepository.GetByIdAsync(userId);
        if (!foundUserRequest.Succeeded || foundUserRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);

        var apiTokenRequest = 
            await _userRepository.GetUserExtendedAttributesByTypeAndValueAsync(userId, ExtendedAttributeType.UserApiToken, value);
        if (!apiTokenRequest.Succeeded || apiTokenRequest.Result is null || !apiTokenRequest.Result.Any())
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var apiToken = apiTokenRequest.Result.FirstOrDefault();
        if (apiToken is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        if (apiToken.OwnerId != foundUserRequest.Result.Id)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
        
        var removeRequest = await _userRepository.RemoveExtendedAttributeAsync(apiToken.Id);
        if (!removeRequest.Succeeded)
            return await Result.FailAsync(removeRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAllUserApiTokens(Guid userId)
    {
        var foundUserRequest = await _userRepository.GetByIdAsync(userId);
        if (!foundUserRequest.Succeeded || foundUserRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);
        
        var existingTokenRequest =
            await _userRepository.GetUserExtendedAttributesByTypeAsync(foundUserRequest.Result.Id, ExtendedAttributeType.UserApiToken);
        if (!existingTokenRequest.Succeeded)
            return await Result.FailAsync(existingTokenRequest.ErrorMessage);

        var errorMessages = new List<string>();
        
        var existingTokens = (existingTokenRequest.Result ?? new List<AppUserExtendedAttributeDb>()).ToArray();
        if (existingTokens.Any())
        {
            foreach (var token in existingTokens)
            {
                var removeRequest = await _userRepository.RemoveExtendedAttributeAsync(token.Id);
                if (!removeRequest.Succeeded)
                    errorMessages.Add(removeRequest.ErrorMessage);
            }
        }

        if (errorMessages.Any())
            return await Result.FailAsync(errorMessages);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> SetExternalAuthProvider(Guid userId, ExternalAuthProvider provider, string externalId)
    {
        var foundUserRequest = await _userRepository.GetByIdAsync(userId);
        if (!foundUserRequest.Succeeded || foundUserRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);

        // We should only have one provider binding per account, we'll just wipe out any that exist just in case
        await RemoveExternalAuthProvider(userId, provider);

        var createAuthRequest = await _userRepository.AddExtendedAttributeAsync(new AppUserExtendedAttributeCreate
        {
            OwnerId = userId,
            Name = "ExternalAuthProviderBinding",
            Value = externalId,
            Description = provider.ToString(),
            Type = ExtendedAttributeType.ExternalAuthLogin
        });
        if (!createAuthRequest.Succeeded)
            return await Result.FailAsync(createAuthRequest.ErrorMessage);

        return await Result.SuccessAsync($"Successfully bound provider {provider.ToString()} to the account");
    }

    public async Task<IResult> RemoveExternalAuthProvider(Guid userId, ExternalAuthProvider provider)
    {
        var foundUserRequest = await _userRepository.GetByIdAsync(userId);
        if (!foundUserRequest.Succeeded || foundUserRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);

        var existingProviderRequest = 
            await _userRepository.GetUserExtendedAttributesByTypeAsync(userId, ExtendedAttributeType.ExternalAuthLogin);
        if (!existingProviderRequest.Succeeded || existingProviderRequest.Result is null || !existingProviderRequest.Result.Any())
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var errorMessages = new List<string>();
        
        foreach (var authEntry in existingProviderRequest.Result)
        {
            if (authEntry.Description != provider.ToString())
                continue;

            var removeRequest = await _userRepository.RemoveExtendedAttributeAsync(authEntry.Id);
            if (!removeRequest.Succeeded)
                errorMessages.Add(removeRequest.ErrorMessage);
        }

        if (errorMessages.Any())
            return await Result.FailAsync(errorMessages);

        return await Result.SuccessAsync($"Successfully unbound external provider {provider.ToString()} from account");
    }
}