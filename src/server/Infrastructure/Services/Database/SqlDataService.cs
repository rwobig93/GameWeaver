using System.Data;
using Application.Database;
using Application.Database.MsSql;
using Application.Database.Postgres;
using Application.Helpers.Runtime;
using Application.Services.Database;
using Application.Settings.AppSettings;
using Dapper;
using Domain.Contracts;
using Domain.Enums.Database;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Services.Database;

public class SqlDataService : ISqlDataService
{
    private readonly DatabaseConfiguration _dbConfig;
    private readonly ILogger _logger;

    public SqlDataService(IOptions<DatabaseConfiguration> dbConfig, ILogger logger)
    {
        _dbConfig = dbConfig.Value;
        _logger = logger;
    }

    public void EnforceDatabaseStructure(string connectionId)
    {
        EnforceDatabaseEntities();
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

    public async Task<int> SaveData<TParameters>(ISqlDatabaseScript script, TParameters parameters, string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.ExecuteAsync(script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<Guid> SaveDataReturnId<TParameters>(
        ISqlDatabaseScript script, TParameters parameters, string connectionId = "DefaultConnection", int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.ExecuteScalarAsync<Guid>(
            script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<int> SaveDataReturnIntId<TParameters>(
        ISqlDatabaseScript script, TParameters parameters, string connectionId = "DefaultConnection", int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.ExecuteScalarAsync<int>(
            script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<IEnumerable<TDataClass>> LoadData<TDataClass, TParameters>(ISqlDatabaseScript script, TParameters parameters,
        string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.QueryAsync<TDataClass>(
            script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<PaginatedDbEntity<IEnumerable<TDataClass>>> LoadDataPaginated<TDataClass, TParameters>(ISqlDatabaseScript script, TParameters parameters,
        string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        var response = (await connection.QueryAsync<int, TDataClass, (int, TDataClass)>(
            script.Path, (totalCount, entity) => (totalCount, entity), parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds)).ToArray();

        return new PaginatedDbEntity<IEnumerable<TDataClass>> { Data = response.Select(x => x.Item2), TotalCount = response.FirstOrDefault().Item1 };
    }

    public async Task<IEnumerable<TDataClass>> LoadDataJoin<TDataClass, TDataClassJoin, TParameters>(ISqlDatabaseScript script,
        Func<TDataClass, TDataClassJoin, TDataClass> joinMapping, TParameters parameters, string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.QueryAsync(
            script.Path, map: joinMapping, param: parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<IEnumerable<TDataClass>> LoadDataJoin<TDataClass, TDataClassJoinOne, TDataClassJoinTwo, TParameters>(
        ISqlDatabaseScript script,  Func<TDataClass, TDataClassJoinOne, TDataClassJoinTwo, TDataClass> joinMapping,
        TParameters parameters, string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());

        return await connection.QueryAsync(
            script.Path, map: joinMapping, param: parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    private void ExecuteSqlScriptObject(ISqlDatabaseScript dbEntity)
    {
        try
        {
            using IDbConnection connection = new SqlConnection(GetCurrentConnectionString());
            connection.Execute(dbEntity.SqlStatement);
            _logger.Debug("Sql Enforce Success: [Type]{scriptType} [Name]{scriptName}",
                dbEntity.Type, dbEntity.FriendlyName);
        }
        catch (Exception ex)
        {
            _logger.Error("Sql Enforce Fail: [Type]{scriptType} [Name]{scriptName} :: {errorMessage}",
                dbEntity.Type, dbEntity.FriendlyName, ex.Message);
        }
    }

    private void EnforceDatabaseEntities()
    {
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
            databaseScripts.AddRange(entity.GetDbScripts());

        // Sort scripts in order of indicated enforcement
        databaseScripts.Sort((scriptOne, scriptTwo) =>
        {
            // Sort by EnforcementOrder in descending order
            var orderWinner = scriptOne.EnforcementOrder.CompareTo(scriptTwo.EnforcementOrder);

            return orderWinner != 0 ? orderWinner :
                // EnforcementOrder matches on both comparable objects, secondary sort by Table Name in Descending order
                string.Compare(scriptOne.FriendlyName, scriptTwo.FriendlyName, StringComparison.Ordinal);
        });

        foreach (var script in databaseScripts)
            ExecuteSqlScriptObject(script);
    }
}