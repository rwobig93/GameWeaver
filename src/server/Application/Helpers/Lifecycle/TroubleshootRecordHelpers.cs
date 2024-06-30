using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Lifecycle;
using Application.Services.System;
using Domain.Contracts;
using Domain.Enums.Lifecycle;
using Newtonsoft.Json;

namespace Application.Helpers.Lifecycle;

public static class TroubleshootRecordHelpers
{
    public static async Task<IResult<Guid>> CreateTroubleshootRecord(this ITroubleshootingRecordService tshootService, IDateTimeService dateTime, TroubleshootEntityType type,
        Guid recordId, Guid changedById, string message, Dictionary<string, string>? detail)
    {
        var recordCreate = await tshootService.CreateAsync(new TroubleshootingRecordCreate
        {
            EntityType = type,
            RecordId = recordId,
            ChangedBy = changedById,
            Timestamp = dateTime.NowDatabaseTime,
            Message = message,
            Detail = detail is null ? JsonConvert.SerializeObject(new Dictionary<string, string>()) : JsonConvert.SerializeObject(detail)
        });

        return await Result<Guid>.SuccessAsync(recordCreate.Data);
    }
    
    public static async Task<IResult<Guid>> CreateTroubleshootRecord(this ITroubleshootingRecordService tshootService, IRunningServerState serverState, IDateTimeService dateTime,
        TroubleshootEntityType type, Guid recordId, string message, Dictionary<string, string>? detail)
    {
        return await CreateTroubleshootRecord(tshootService, dateTime, type, recordId, serverState.SystemUserId, message, detail);
    }
    
    public static async Task<IResult<Guid>> CreateTroubleshootRecord(this ITroubleshootingRecordsRepository tshootRepository, IDateTimeService dateTime, TroubleshootEntityType type,
        Guid recordId, Guid changedById, string message, Dictionary<string, string>? detail)
    {
        var recordCreate = await tshootRepository.CreateAsync(new TroubleshootingRecordCreate
        {
            EntityType = type,
            RecordId = recordId,
            ChangedBy = changedById,
            Timestamp = dateTime.NowDatabaseTime,
            Message = message,
            Detail = detail is null ? JsonConvert.SerializeObject(new Dictionary<string, string>()) : JsonConvert.SerializeObject(detail)
        });

        return await Result<Guid>.SuccessAsync(recordCreate.Result);
    }
    
    public static async Task<IResult<Guid>> CreateTroubleshootRecord(this ITroubleshootingRecordsRepository tshootRepository, IRunningServerState serverState,
        IDateTimeService dateTime, TroubleshootEntityType type, Guid recordId, string message, Dictionary<string, string>? detail)
    {
        return await tshootRepository.CreateTroubleshootRecord(dateTime, type, recordId, serverState.SystemUserId, message, detail);
    }
}