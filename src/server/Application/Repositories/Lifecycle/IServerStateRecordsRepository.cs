using Application.Models.Lifecycle;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Models.Database;

namespace Application.Repositories.Lifecycle;

public interface IServerStateRecordsRepository
{
    Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetAllBeforeDateAsync(DateTime olderThan);
    Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetAllAfterDateAsync(DateTime newerThan);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<ServerStateRecordDb>> GetLatestAsync();
    Task<DatabaseActionResult<ServerStateRecordDb>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetByAppVersionAsync(Version version);
    Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetByDatabaseVersionAsync(Version version);
    Task<DatabaseActionResult<Guid>> CreateAsync(ServerStateRecordCreate createRecord);
}