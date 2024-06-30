using System.Data;
using Application.Constants.Identity;
using Application.Database;
using Application.Database.MsSql;
using Application.Database.Postgres;
using Application.Helpers.Identity;
using Application.Helpers.Runtime;
using Application.Helpers.Web;
using Application.Mappers.Identity;
using Application.Mappers.Lifecycle;
using Application.Models.Identity.Role;
using Application.Models.Identity.User;
using Application.Models.Identity.UserExtensions;
using Application.Models.Lifecycle;
using Application.Repositories.Identity;
using Application.Repositories.Lifecycle;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Dapper;
using Domain.DatabaseEntities.Identity;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Enums.Database;
using Domain.Enums.Identity;
using Domain.Models.Database;
using Domain.Models.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Services.Database;

public class SqlDatabaseSeederService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IAppUserRepository _userRepository;
    private readonly IAppRoleRepository _roleRepository;
    private readonly IAppPermissionRepository _permissionRepository;
    private readonly LifecycleConfiguration _lifecycleConfig;
    private readonly IRunningServerState _serverState;
    private readonly SecurityConfiguration _securityConfig;
    private readonly IServerStateRecordsRepository _serverStateRepository;
    private readonly IDateTimeService _dateTime;
    private readonly DatabaseConfiguration _dbConfig;

    private AppUserSecurityDb _systemUser = new() { Id = Guid.Empty };

    public SqlDatabaseSeederService(ILogger logger, IAppUserRepository userRepository, IAppRoleRepository roleRepository,
        IAppPermissionRepository permissionRepository, IOptions<LifecycleConfiguration> lifecycleConfig, IRunningServerState serverState,
        IOptions<SecurityConfiguration> securityConfig, IServerStateRecordsRepository serverStateRepository, IDateTimeService dateTime,
        IOptions<DatabaseConfiguration> dbConfig)
    {
        _logger = logger;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _lifecycleConfig = lifecycleConfig.Value;
        _serverState = serverState;
        _serverStateRepository = serverStateRepository;
        _dateTime = dateTime;
        _dbConfig = dbConfig.Value;
        _securityConfig = securityConfig.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Debug("Starting to seed database");
        await EnforceServerStateRecords();
        await ApplyDatabaseMigrations();
        await SeedSystemUser();
        await SeedDatabaseRoles();
        await SeedDatabaseUsers();
        _logger.Information("Finished seeding database");
    }

    private async Task SeedSystemUser()
    {
        var systemUser = await CreateOrGetSeedUser(
            UserConstants.DefaultUsers.SystemUsername, UserConstants.DefaultUsers.SystemFirstName, UserConstants.DefaultUsers.SystemLastName,
            UserConstants.DefaultUsers.SystemEmail, UrlHelpers.GenerateToken(64));
        _systemUser = systemUser.Result!;

        _serverState.UpdateSystemUserId(_systemUser.Id);
    }

    private async Task SeedDatabaseRoles()
    {
        var adminRole = await CreateOrGetSeedRole(
            RoleConstants.DefaultRoles.AdminName, RoleConstants.DefaultRoles.AdminDescription);
        if (adminRole.Succeeded)
            await EnforcePermissionsForRole(adminRole.Result!.Id, PermissionHelpers.GetAllBuiltInPermissions());
        
        var moderatorRole = await CreateOrGetSeedRole(
            RoleConstants.DefaultRoles.ModeratorName, RoleConstants.DefaultRoles.ModeratorDescription);
        if (moderatorRole.Succeeded && _lifecycleConfig.EnforceDefaultRolePermissions)
            await EnforcePermissionsForRole(moderatorRole.Result!.Id, PermissionHelpers.GetModeratorRolePermissions());
        
        var serviceAccountRole = await CreateOrGetSeedRole(
            RoleConstants.DefaultRoles.ServiceAccountName, RoleConstants.DefaultRoles.ServiceAccountDescription);
        if (serviceAccountRole.Succeeded && _lifecycleConfig.EnforceDefaultRolePermissions)
            await EnforcePermissionsForRole(serviceAccountRole.Result!.Id, PermissionHelpers.GetServiceAccountRolePermissions());

        var defaultRole = await CreateOrGetSeedRole(
            RoleConstants.DefaultRoles.DefaultName, RoleConstants.DefaultRoles.DefaultDescription);
        if (defaultRole.Succeeded && _lifecycleConfig.EnforceDefaultRolePermissions)
            await EnforcePermissionsForRole(defaultRole.Result!.Id, PermissionHelpers.GetDefaultRolePermissions());
    }

    private async Task SeedDatabaseUsers()
    {
        // Seed system user permissions
        await EnforceRolesForUser(_systemUser.Id, RoleConstants.GetAdminRoleNames());
        
        var adminUser = await CreateOrGetSeedUser(
            UserConstants.DefaultUsers.AdminUsername, UserConstants.DefaultUsers.AdminFirstName, UserConstants.DefaultUsers.AdminLastName,
            UserConstants.DefaultUsers.AdminEmail, UserConstants.DefaultUsers.AdminPassword);
        if (adminUser.Succeeded)
            await EnforceRolesForUser(adminUser.Result!.Id, RoleConstants.GetAdminRoleNames());

        if (_lifecycleConfig.EnforceTestAccounts)
        {
            var moderatorUser = await CreateOrGetSeedUser(
                UserConstants.DefaultUsers.ModeratorUsername, UserConstants.DefaultUsers.ModeratorFirstName,
                UserConstants.DefaultUsers.ModeratorLastName, UserConstants.DefaultUsers.ModeratorEmail, UserConstants.DefaultUsers.ModeratorPassword);
            if (moderatorUser.Succeeded)
                await EnforceRolesForUser(moderatorUser.Result!.Id, RoleConstants.GetModeratorRoleNames());
            
            var basicUser = await CreateOrGetSeedUser(
                UserConstants.DefaultUsers.BasicUsername, UserConstants.DefaultUsers.BasicFirstName, UserConstants.DefaultUsers.BasicLastName,
                UserConstants.DefaultUsers.BasicEmail, UserConstants.DefaultUsers.BasicPassword);
            if (basicUser.Succeeded)
                await EnforceRolesForUser(basicUser.Result!.Id, RoleConstants.GetDefaultRoleNames());
        }
        
        var anonymousUser = await CreateOrGetSeedUser(
            UserConstants.DefaultUsers.AnonymousUsername, UserConstants.DefaultUsers.AnonymousFirstName,
            UserConstants.DefaultUsers.AnonymousLastName, UserConstants.DefaultUsers.AnonymousEmail, UrlHelpers.GenerateToken(64));
        if (anonymousUser.Succeeded)
            await EnforceAnonUserIdToEmptyGuid(anonymousUser.Result!.Id);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // We don't have any cleanup required, so we'll just return a completed task
        await Task.CompletedTask;
    }

    private async Task<DatabaseActionResult<AppRoleDb>> CreateOrGetSeedRole(string roleName, string roleDescription)
    {
        var existingRole = await _roleRepository.GetByNameAsync(roleName);
        if (!existingRole.Succeeded)
            return existingRole;
        if (existingRole.Result is not null)
            return existingRole;
        
        var createdRole = await _roleRepository.CreateAsync(new AppRoleCreate()
        {
            Name = roleName,
            Description = roleDescription,
            CreatedOn = DateTime.Now,
            CreatedBy = _systemUser.Id
        });
        _logger.Information("Created missing {RoleName} role with id: {RoleId}", roleName, createdRole.Result);

        return await _roleRepository.GetByIdAsync(createdRole.Result);
    }

    private async Task EnforcePermissionsForRole(Guid roleId, List<string> desiredPermissions)
    {
        var currentPermissions = await _permissionRepository.GetAllForRoleAsync(roleId);
        foreach (var permission in desiredPermissions)
        {
            if (currentPermissions.Result!.Any(x => x.ClaimValue == permission))
                continue;

            var convertedPermission = permission.ToAppPermissionCreate();
            convertedPermission.RoleId = roleId;
            convertedPermission.CreatedBy = _systemUser.Id;
            var addedPermission = await _permissionRepository.CreateAsync(convertedPermission);
            if (!addedPermission.Succeeded)
            {
                _logger.Error("Failed to enforce permission {PermissionValue} on role {RoleId}: {ErrorMessage}",
                    permission, roleId, addedPermission.ErrorMessage);
                continue;
            }
            
            _logger.Debug("Added missing {PermissionValue} to role {RoleId} with id {PermissionId}",
                permission, roleId, addedPermission.Result);
        }
    }

    private async Task<DatabaseActionResult<AppUserSecurityDb>> CreateOrGetSeedUser(string userName, string firstName, string lastName, string email,
        string userPassword)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(userName);
        if (!existingUser.Succeeded)
        {
            _logger.Error("Failed to seed user in database: {UserName} => {ErrorMessage}", userName, existingUser.ErrorMessage);
            return existingUser;
        }
        if (existingUser.Result is not null)
            return existingUser;
        
        var createdUser = await _userRepository.CreateAsync(new AppUserCreate
        {
            Username = userName,
            Email = email,
            EmailConfirmed = true,
            PhoneNumber = "",
            PhoneNumberConfirmed = false,
            FirstName = firstName,
            LastName = lastName,
            ProfilePictureDataUrl = null,
            CreatedBy = _systemUser.Id,
            CreatedOn = DateTime.Now,
            LastModifiedBy = null,
            LastModifiedOn = null,
            IsDeleted = false,
            DeletedOn = null,
            AccountType = AccountType.User
        });
        
        AccountHelpers.GenerateHashAndSalt(userPassword, _securityConfig.PasswordPepper, out var salt, out var hash);
        await _userRepository.UpdateSecurityAsync(new AppUserSecurityAttributeUpdate
        {
            OwnerId = createdUser.Result,
            PasswordHash = hash,
            PasswordSalt = salt,
            TwoFactorEnabled = false,
            TwoFactorKey = null,
            AuthState = AuthState.Enabled,
            AuthStateTimestamp = null,
            BadPasswordAttempts = 0,
            LastBadPassword = null
        });
        
        _logger.Information("Created missing {UserName} user with id: {UserId}", userName, createdUser.Result);
        
        return await _userRepository.GetByIdAsync(createdUser.Result);
    }

    private async Task EnforceRolesForUser(Guid userId, IEnumerable<string> roleNames)
    {
        var currentRoles = await _roleRepository.GetRolesForUser(userId);
        foreach (var role in roleNames.Where(role => !currentRoles.Result!.Any(x => x.Name == role)))
        {
            var foundRole = await _roleRepository.GetByNameAsync(role);
            await _roleRepository.AddUserToRoleAsync(userId, foundRole.Result!.Id, _systemUser.Id);
            
            _logger.Debug("Added missing role {RoleId} to user {UserId}", foundRole.Result.Id, userId);
        }
    }

    private async Task EnforceAnonUserIdToEmptyGuid(Guid currentId)
    {
        var desiredAnonUser = await _userRepository.GetByIdAsync(Guid.Empty);
        if (desiredAnonUser.Result is not null)
            return;
        
        var updatedId = await _userRepository.SetUserId(currentId, Guid.Empty);
        if (!updatedId.Succeeded)
        {
            _logger.Error("Failed to set Anonymous UserId to Empty Guid: {ErrorMessage}", updatedId.ErrorMessage);
            return;
        }
        
        var anonUserValidation = await _userRepository.GetByIdAsync(Guid.Empty);
        if (anonUserValidation.Result is null)
        {
            _logger.Error("Failed to get Anonymous UserId after update: {ErrorMessage}", anonUserValidation.ErrorMessage);
            return;
        }
        
        _logger.Information("Anon user ID was updated and validated correct: {UserId}", anonUserValidation.Result!.Id);
    }

    private async Task EnforceServerStateRecords()
    {
        var existingStateRecord = await _serverStateRepository.GetLatestAsync();
        if (!existingStateRecord.Succeeded)
        {
            _logger.Information("Failed to retrieve existing server state record, this is expected on the first app run: {Error}",
                existingStateRecord.ErrorMessage);
        }

        var latestRecord = existingStateRecord.Result;
        if (latestRecord is not null)
        {
            _logger.Debug("Existing server state record exists => [{Id}]{Version}/{DatabaseVersion} :: {Timestamp}",
                latestRecord.Id, latestRecord.AppVersion, latestRecord.DatabaseVersion, latestRecord.Timestamp);
        
            if (new Version(latestRecord.AppVersion) == _serverState.ApplicationVersion)
            {
                _logger.Debug($"App version hasn't changed and hasn't been upgraded");
                return;
            }

            // Update the application version while keeping the database version for database migrations
            latestRecord.AppVersion = _serverState.ApplicationVersion.ToString();
        }
        else
        {
            // Create the first record for the database
            latestRecord = new ServerStateRecordDb()
            {
                Timestamp = _dateTime.NowDatabaseTime,
                AppVersion = _serverState.ApplicationVersion.ToString(),
                DatabaseVersion = "0.0.0.0"
            };
        }
        
        var createRecordRequest = await _serverStateRepository.CreateAsync(latestRecord.ToCreate());
        if (!createRecordRequest.Succeeded)
        {
            _logger.Error("Failed to create server state record: {Error}", createRecordRequest.ErrorMessage);
            return;
        }
        
        _logger.Information("Application updated, created server state record: {Id} {AppVersion}/{DatabaseVersion}",
            createRecordRequest.Result, latestRecord.AppVersion, latestRecord.DatabaseVersion);
    }

    private void ExecuteSqlMigration(ISqlMigration migration, bool databaseUpgrade)
    {
        try
        {
            using IDbConnection connection = new SqlConnection(_dbConfig.MsSql);
            connection.Execute(databaseUpgrade ? migration.Up : migration.Down);
            _logger.Debug("Sql Migration Success: [Upgrade]{DatabaseUpgrade} [TargetVersion]{TargetVersion}", databaseUpgrade, migration.VersionTarget);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Sql Migration Failure: [Upgrade]{DatabaseUpgrade} [TargetVersion]{TargetVersion}", databaseUpgrade, migration.VersionTarget);
        }
    }

    private async Task ApplyDatabaseMigrations()
    {
        var existingStateRecord = await _serverStateRepository.GetLatestAsync();
        if (!existingStateRecord.Succeeded || existingStateRecord.Result is null)
        {
            _logger.Error("Failed to retrieve existing server state record: {Error}", existingStateRecord.ErrorMessage);
            return;
        }
        
        var databaseMigrations = _dbConfig.Provider switch
        {
            DatabaseProviderType.MsSql => typeof(IMsSqlMigration).GetImplementingTypes<ISqlMigration>(),
            DatabaseProviderType.Postgresql => typeof(IPostgresqlMigration).GetImplementingTypes<ISqlMigration>(),
            _ => typeof(IMsSqlMigration).GetImplementingTypes<ISqlMigration>()
        };
        var databaseUpgrade = new Version(existingStateRecord.Result.AppVersion) > new Version(existingStateRecord.Result.DatabaseVersion);

        if (!databaseUpgrade)
        {
            _logger.Information($"App & Database version match, no migrations necessary, skipping DB migrations");
            return;
        }

        databaseMigrations = databaseUpgrade
            // Filter migrations in ascending order that are newer than the database version but equal or older than the app version
            ? databaseMigrations.OrderBy(x => x.VersionTarget)
                .Where(x => x.VersionTarget > new Version(existingStateRecord.Result.DatabaseVersion)
                            && x.VersionTarget <= new Version(existingStateRecord.Result.AppVersion)).ToArray()
            // Filter migrations in descending order that are older than the database version but equal or newer than the app version
            : databaseMigrations.OrderByDescending(x => x.VersionTarget)
                .Where(x => x.VersionTarget < new Version(existingStateRecord.Result.DatabaseVersion)
                            && x.VersionTarget >= new Version(existingStateRecord.Result.AppVersion)).ToArray();

        foreach (var migration in databaseMigrations)
            ExecuteSqlMigration(migration, databaseUpgrade);

        var newRecord = new ServerStateRecordCreate
        {
            Timestamp = _dateTime.NowDatabaseTime,
            AppVersion = _serverState.ApplicationVersion.ToString(),
            DatabaseVersion = _serverState.ApplicationVersion.ToString()
        };

        var createRecordRequest = await _serverStateRepository.CreateAsync(newRecord);
        if (!createRecordRequest.Succeeded)
        {
            _logger.Error("Failed to create server state record: {Error}", createRecordRequest.ErrorMessage);
            return;
        }
        
        _logger.Information("Updated server state record post DB migration: {Id} {AppVersion}/{DatabaseVersion}",
            createRecordRequest.Result, newRecord.AppVersion, newRecord.DatabaseVersion);
    }
}