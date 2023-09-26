using Application.Database;

namespace Application.Services.Database;

public interface ISqlDataService
{
    public void EnforceDatabaseStructure(string connectionId = "DefaultConnection");

    public Task<int> SaveData<TParameters>(ISqlDatabaseScript script, TParameters parameters, string connectionId = "DefaultConnection",
        int timeoutSeconds = 5);

    public Task<Guid> SaveDataReturnId<TParameters>(ISqlDatabaseScript script, TParameters parameters,  string connectionId = "DefaultConnection",
        int timeoutSeconds = 5);
    
    public Task<IEnumerable<TDataClass>> LoadData<TDataClass, TParameters>(ISqlDatabaseScript script, TParameters parameters,
        string connectionId = "DefaultConnection", int timeoutSeconds = 5);
    
    public Task<IEnumerable<TDataClass>> LoadDataJoin<TDataClass, TDataClassJoin, TParameters>(ISqlDatabaseScript script,
        Func<TDataClass, TDataClassJoin, TDataClass> joinMapping, TParameters parameters, string connectionId = "DefaultConnection",
        int timeoutSeconds = 5);
    
    public Task<IEnumerable<TDataClass>> LoadDataJoin<TDataClass, TDataClassJoinOne, TDataClassJoinTwo, TParameters>(ISqlDatabaseScript script,
        Func<TDataClass, TDataClassJoinOne, TDataClassJoinTwo, TDataClass> joinMapping, TParameters parameters,
        string connectionId = "DefaultConnection", int timeoutSeconds = 5);
}