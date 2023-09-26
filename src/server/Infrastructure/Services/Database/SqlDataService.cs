using System.Data;
using System.Data.SqlClient;
using Application.Database;
using Application.Database.Providers;
using Application.Helpers.Runtime;
using Application.Services.Database;
using Application.Settings.AppSettings;
using Dapper;
using Domain.Enums.Database;
using Microsoft.Extensions.Options;

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

    public async Task<int> SaveData<TParameters>(ISqlDatabaseScript script, TParameters parameters, string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(_dbConfig.Core);

        return await connection.ExecuteAsync(script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<Guid> SaveDataReturnId<TParameters>(
        ISqlDatabaseScript script, TParameters parameters, string connectionId = "DefaultConnection", int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(_dbConfig.Core);

        return await connection.ExecuteScalarAsync<Guid>(
            script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<IEnumerable<TDataClass>> LoadData<TDataClass, TParameters>(ISqlDatabaseScript script, TParameters parameters,
        string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(_dbConfig.Core);

        return await connection.QueryAsync<TDataClass>(
            script.Path, parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }
    
    public async Task<IEnumerable<TDataClass>> LoadDataJoin<TDataClass, TDataClassJoin, TParameters>(ISqlDatabaseScript script,
        Func<TDataClass, TDataClassJoin, TDataClass> joinMapping, TParameters parameters, string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(_dbConfig.Core);

        return await connection.QueryAsync(
            script.Path, map: joinMapping, param: parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    public async Task<IEnumerable<TDataClass>> LoadDataJoin<TDataClass, TDataClassJoinOne, TDataClassJoinTwo, TParameters>(
        ISqlDatabaseScript script,  Func<TDataClass, TDataClassJoinOne, TDataClassJoinTwo, TDataClass> joinMapping,
        TParameters parameters, string connectionId, int timeoutSeconds = 5)
    {
        using IDbConnection connection = new SqlConnection(_dbConfig.Core);

        return await connection.QueryAsync(
            script.Path, map: joinMapping, param: parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
    }

    private void ExecuteSqlScriptObject(ISqlDatabaseScript dbEntity)
    {
        try
        {
            using IDbConnection connection = new SqlConnection(_dbConfig.Core);
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