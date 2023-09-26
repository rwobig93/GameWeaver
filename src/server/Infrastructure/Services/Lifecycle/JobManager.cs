using Application.Repositories.Identity;
using Application.Services.Identity;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Enums.Identity;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Lifecycle;

public class JobManager : IJobManager
{
    private readonly ILogger _logger;
    private readonly IAppUserRepository _userRepository;
    private readonly IAppAccountService _accountService;
    private readonly IDateTimeService _dateTime;
    private readonly SecurityConfiguration _securityConfig;
    private readonly IAuditTrailService _auditService;
    private readonly LifecycleConfiguration _lifecycleConfig;

    public JobManager(ILogger logger, IAppUserRepository userRepository, IAppAccountService accountService, IDateTimeService dateTime,
        IOptions<SecurityConfiguration> securityConfig, IAuditTrailService auditService, IOptions<LifecycleConfiguration> lifecycleConfig)
    {
        _logger = logger;
        _userRepository = userRepository;
        _accountService = accountService;
        _dateTime = dateTime;
        _auditService = auditService;
        _lifecycleConfig = lifecycleConfig.Value;
        _securityConfig = securityConfig.Value;
    }

    public async Task UserHousekeeping()
    {
        try
        {
            await HandleLockedOutUsers();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "User housekeeping failed: {Error}", ex.Message);
        }
    }

    public async Task DailyCleanup()
    {
        try
        {
            var auditCleanup = await _auditService.DeleteOld(_lifecycleConfig.AuditLogLifetime);
            if (!auditCleanup.Succeeded)
                _logger.Error("Audit cleanup failed: {Error}", auditCleanup.Messages);
            
            _logger.Debug("Finished daily cleanup");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Daily cleanup failed: {Error}", ex.Message);
        }
    }

    private async Task HandleLockedOutUsers()
    {
        // If account lockout threshold is 0 minutes then accounts are locked until unlocked by an administrator
        if (_securityConfig.AccountLockoutMinutes == 0)
            return;

        var allLockedOutUsers = await _userRepository.GetAllLockedOutAsync();
        if (!allLockedOutUsers.Succeeded || allLockedOutUsers.Result is null)
        {
            _logger.Error("Failed to get locked out users: {Error}", allLockedOutUsers.ErrorMessage);
            return;
        }

        if (!allLockedOutUsers.Result.Any())
        {
            _logger.Debug("Currently no locked out users found during user housekeeping");
            return;
        }

        foreach (var user in allLockedOutUsers.Result)
        {
            try
            {
                // Account hasn't reached lockout threshold, skipping for now
                if (user.AuthStateTimestamp!.Value.AddMinutes(_securityConfig.AccountLockoutMinutes) < _dateTime.NowDatabaseTime)
                    continue;
                
                // Account has passed locked threshold so it's ready to be unlocked
                var unlockResult = await _accountService.SetAuthState(user.Id, AuthState.Enabled);
                if (!unlockResult.Succeeded)
                {
                    _logger.Error("Failed to unlock user during housekeeping: [{Id}]{Username}", user.Id, user.Username);
                    continue;
                }
                
                _logger.Debug("Successfully unlocked locked account that passed the lockout threshold[{Id}]{Username}", 
                    user.Id, user.Username);
            }
            catch (Exception innerException)
            {
                _logger.Error(innerException, 
                    "Failed to parse locked out user for housekeeping: [{Id}]{Username}", user.Id, user.Username);
            }
        }
        
        _logger.Debug("Finished handling locked out users during housekeeping");
    }
}