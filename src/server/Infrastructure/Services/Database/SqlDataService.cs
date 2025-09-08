using System.Data;
using Application.Database;
using Application.Database.MsSql;
using Application.Database.Postgres;
using Application.Helpers.Runtime;
using Application.Services.Database;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Dapper;
using Domain.Contracts;
using Domain.DatabaseEntities._Management;
using Domain.Enums.Database;
using Infrastructure.Database.MsSql._Management;
using Infrastructure.Database.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Database;

public class SqlDataService : ISqlDataService
{
    private readonly DatabaseConfiguration _dbConfig;
    private readonly ILogger _logger;
    private readonly IRunningServerState _serverState;
    private readonly IDateTimeService _dateTime;

    public SqlDataService(IOptions<DatabaseConfiguration> dbConfig, ILogger logger, IRunningServerState serverState, IDateTimeService dateTime)
    {
        _dbConfig = dbConfig.Value;
        _logger = logger;
        _serverState = serverState;
        _dateTime = dateTime;
    }

    public void EnforceDatabaseStructure()
    {
        EnforceDatabaseEntities().GetAwaiter().GetResult();
    }

    private string GetCurrentConnectionString()
    {
        return _dbConfig.Provider switch
        {
            DatabaseProviderType.MsSql => _dbConfig.MsSql,
            DatabaseProviderType.Postgresql => _dbConfig.Postgres,
            DatabaseProviderType.Sqlite => _dbConfig.Sqlite,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<int> SaveData<TParameters>(ISqlDatabaseScript script, TParameters parameters, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.ExecuteAsync(script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<Guid> SaveDataReturnId<TParameters>(ISqlDatabaseScript script, TParameters parameters, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.ExecuteScalarAsync<Guid>(script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<int> SaveDataReturnIntId<TParameters>(ISqlDatabaseScript script, TParameters parameters, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.ExecuteScalarAsync<int>(script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<IEnumerable<TDataClass>> LoadData<TDataClass, TParameters>(ISqlDatabaseScript script, TParameters parameters, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.QueryAsync<TDataClass>(script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<PaginatedDbEntity<IEnumerable<TDataClass>>> LoadDataPaginated<TDataClass, TParameters>(ISqlDatabaseScript script, TParameters parameters,
        int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        var response = (await connection.QueryAsync<int, TDataClass, (int, TDataClass)>(script.Path, (totalCount, entity) => (totalCount, entity),
            parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds)).ToArray();

        return new PaginatedDbEntity<IEnumerable<TDataClass>> {Data = response.Select(x => x.Item2), TotalCount = response.FirstOrDefault().Item1};
    }

    public async Task<IEnumerable<TDataClass>> LoadDataJoin<TDataClass, TDataClassJoin, TParameters>(ISqlDatabaseScript script,
        Func<TDataClass, TDataClassJoin, TDataClass> joinMapping, TParameters parameters, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.QueryAsync(script.Path, map: joinMapping, param: parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<IEnumerable<TDataClass>> LoadDataJoin<TDataClass, TDataClassJoinOne, TDataClassJoinTwo, TParameters>(
        ISqlDatabaseScript script, Func<TDataClass, TDataClassJoinOne, TDataClassJoinTwo, TDataClass> joinMapping,
        TParameters parameters, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.QueryAsync(script.Path, map: joinMapping, param: parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<IEnumerable<TDataType>> ExecuteStatement<TDataType>(string statement, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        try
        {
            var response = await connection.QueryAsync<TDataType>(statement, commandTimeout: timeoutSeconds);
            return response;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public async Task<TDataType?> ExecuteStatementScalar<TDataType>(string statement, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        try
        {
            var response = await connection.ExecuteScalarAsync<TDataType>(statement, commandTimeout: timeoutSeconds);
            return response;
        }
        catch (Exception)
        {
            return default;
        }
    }

    private async Task<bool> ExecuteSqlScriptObject(ISqlDatabaseScript dbEntity)
    {
        try
        {
            using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());
            await connection.ExecuteAsync(dbEntity.SqlStatement);
            _logger.Debug("Sql Enforce Success: [Type]{scriptType} [Name]{scriptName}", dbEntity.Type, dbEntity.FriendlyName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Sql Enforce Fail: [Type]{scriptType} [Name]{scriptName} :: {errorMessage}", dbEntity.Type, dbEntity.FriendlyName, ex.Message);
            return false;
        }
    }

    private async Task EnforceDatabaseManagement()
    {
        var entitiesToBeEnforced = _dbConfig.Provider switch
        {
            DatabaseProviderType.MsSql => typeof(IMsSqlManagementEntity).GetImplementingTypes<ISqlEnforcedEntity>(),
            DatabaseProviderType.Postgresql => typeof(IPgSqlManagementEntity).GetImplementingTypes<ISqlEnforcedEntity>(),
            _ => typeof(IMsSqlManagementEntity).GetImplementingTypes<ISqlEnforcedEntity>()
        };

        var databaseScripts = new List<ISqlDatabaseScript>();

        // Gather static Database Scripts from inheriting classes
        foreach (var entity in entitiesToBeEnforced)
        {
            databaseScripts.AddRange(entity.GetDbScripts());
        }

        // Sort scripts in order of indicated enforcement
        databaseScripts.Sort((scriptOne, scriptTwo) =>
        {
            // Sort by EnforcementOrder in descending order
            var orderWinner = scriptOne.EnforcementOrder.CompareTo(scriptTwo.EnforcementOrder);

            return orderWinner != 0
                ? orderWinner
                :
                // EnforcementOrder matches on both comparable objects, secondary sort by Table Name in Descending order
                string.Compare(scriptOne.FriendlyName, scriptTwo.FriendlyName, StringComparison.Ordinal);
        });

        var storedProcedures = (await ExecuteStatement<string>(GenericSqlQueries.GetStoredProcedures("spEntityManagement"))).ToList();

        // Enforce database tables and stored procedures
        foreach (var script in databaseScripts)
        {
            if (script.Type == DbResourceType.Table)
            {
                var tableExists = await ExecuteStatementScalar<int>(GenericSqlQueries.TableExists("EntityManagement"));
                if (tableExists == 1)
                {
                    continue;
                }
            }

            if (storedProcedures.Contains(script.Path))
            {
                continue;
            }

            await ExecuteSqlScriptObject(script);
        }
    }

    private async Task<List<EntityManagementDb>> GetManagementEntities()
    {
        try
        {
            var allEntities = await LoadData<EntityManagementDb, dynamic>(EntityManagementMsSql.GetAll, new { });
            return allEntities.ToList();
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to gather database management entities, all DB entities will be enforced: {Error}", ex.Message);
            return [];
        }
    }

    private async Task CreateManagementEntity(EntityManagementDb entity)
    {
        try
        {
            await SaveData(EntityManagementMsSql.Insert, entity);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to create DB management entity [{Path}]: {Error}", entity.Path, ex.Message);
        }
    }

    private async Task UpdateManagementEntity(EntityManagementDb entity)
    {
        try
        {
            await SaveData(EntityManagementMsSql.Update, entity);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to update DB management entity [{Path}]: {Error}", entity.Path, ex.Message);
        }
    }

    private async Task EnforceDatabaseEntities()
    {
        await EnforceDatabaseManagement();

        // Gather inheriting classes based on configured database provider
        var entitiesToBeEnforced = _dbConfig.Provider switch
        {
            DatabaseProviderType.MsSql => typeof(IMsSqlEnforcedEntity).GetImplementingTypes<ISqlEnforcedEntity>(),
            DatabaseProviderType.Postgresql => typeof(IPostgresqlEnforcedEntity).GetImplementingTypes<ISqlEnforcedEntity>(),
            _ => typeof(IMsSqlEnforcedEntity).GetImplementingTypes<ISqlEnforcedEntity>()
        };

        var databaseScripts = new List<ISqlDatabaseScript>();

        // Gather static Database Scripts from inheriting classes
        foreach (var entity in entitiesToBeEnforced)
        {
            databaseScripts.AddRange(entity.GetDbScripts());
        }

        // Sort scripts in order of indicated enforcement
        databaseScripts.Sort((scriptOne, scriptTwo) =>
        {
            // Sort by EnforcementOrder in descending order
            var orderWinner = scriptOne.EnforcementOrder.CompareTo(scriptTwo.EnforcementOrder);

            return orderWinner != 0
                ? orderWinner
                :
                // EnforcementOrder matches on both comparable objects, secondary sort by Table Name in Descending order
                string.Compare(scriptOne.FriendlyName, scriptTwo.FriendlyName, StringComparison.Ordinal);
        });

        // Get the current database entity enforcement state, we only want to enforce what doesn't match and ensure we don't enforce for newer app versions
        var currentEntityStates = await GetManagementEntities();

        // Enforce database tables and stored procedures
        foreach (var script in databaseScripts)
        {
            var currentStatementHash = FileHelpers.GetIntegrityHash(script.SqlStatement);
            var matchingState = currentEntityStates.FirstOrDefault(x => string.Equals(x.Path, script.Path, StringComparison.CurrentCultureIgnoreCase));
            if (matchingState is not null)
            {
                if (currentStatementHash == matchingState.Hash)
                {
                    // The current statement hash matches the existing statement hash, no work is needed
                    continue;
                }

                var stateVersion = new Version(matchingState.AppVersion);
                if (stateVersion > _serverState.ApplicationVersion)
                {
                    // Statement in the database is newer than the app version we are running, we won't clobber
                    _logger.Warning("Database entity was created with an application version newer than we are running: {OurVersion} < {DatabaseVersion}",
                        _serverState.ApplicationVersion, stateVersion);
                    continue;
                }
            }

            var enforceSucceeded = await ExecuteSqlScriptObject(script);
            if (!enforceSucceeded)
            {
                _logger.Error("Failed to enforce database entity: [{Path}]{Hash}", script.Path, currentStatementHash);
                continue;
            }

            if (matchingState is null)
            {
                await CreateManagementEntity(new EntityManagementDb
                {
                    Path = script.Path,
                    Type = script.Type,
                    Hash = currentStatementHash,
                    AppVersion = _serverState.ApplicationVersion.ToString(),
                    LastUpdated = _dateTime.NowDatabaseTime
                });
                _logger.Information("Created database entity: [{Path}] {CurrentHash}", script.Path, currentStatementHash);
                continue;
            }

            _logger.Information("Updated database entity: [{Path}]{PreviousHash} => {CurrentHash}", script.Path, matchingState.Hash, currentStatementHash);
            matchingState.Hash = currentStatementHash;
            matchingState.AppVersion = _serverState.ApplicationVersion.ToString();
            matchingState.LastUpdated = _dateTime.NowDatabaseTime;
            await UpdateManagementEntity(matchingState);
        }
    }
}