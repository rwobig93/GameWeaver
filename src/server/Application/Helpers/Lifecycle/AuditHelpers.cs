using Application.Constants.Lifecycle;
using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Lifecycle;
using Application.Services.System;
using Domain.Enums.Lifecycle;
using Newtonsoft.Json;

namespace Application.Helpers.Lifecycle;

public static class AuditHelpers
{
    public static AuditDiff GetAuditDiff<T>(T beforeObj, T afterObj)
    {
        var auditDiff = new AuditDiff();

        var beforeDict = beforeObj?.GetType().GetProperties()
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(beforeObj)?.ToString());

        var afterDict = afterObj!.GetType().GetProperties()
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(afterObj)?.ToString());

        if (beforeDict is null || beforeDict.Keys.Count < 1)
        {
            auditDiff.Before = new Dictionary<string, string>();
            auditDiff.After = new Dictionary<string, string>();
            return auditDiff;
        }

        var changedProps = beforeDict.Keys.Intersect(afterDict.Keys)
            .Where(key =>
                // Remove any keys we know we don't want to audit like Refresh token, LastModifiedBy, ect
                !AuditTrailConstants.DiffPropertiesToIgnore.Contains(key) &&
                afterDict[key] is not null &&
                beforeDict[key] != afterDict[key]
                ).ToList();

        if (changedProps.Count < 1)
        {
            auditDiff.Before = new Dictionary<string, string>();
            auditDiff.After = new Dictionary<string, string>();
            return auditDiff;
        }

        auditDiff.Before = changedProps.ToDictionary(prop => prop, prop => beforeDict[prop])!;
        auditDiff.After = changedProps.ToDictionary(prop => prop, prop => afterDict[prop])!;

        return auditDiff;
    }

    public static async Task CreateAuditTrail(this IAuditTrailService auditService, IDateTimeService dateTime,
        AuditTableName tableName, Guid recordId, Guid changedById, AuditAction action,
        object? before = null, object? after = null)
    {
        before ??= new Dictionary<string, string>();
        after ??= new Dictionary<string, string>();

        await auditService.CreateAsync(new AuditTrailCreate
        {
            Timestamp = dateTime.NowDatabaseTime,
            TableName = tableName.ToString(),
            RecordId = recordId,
            ChangedBy = changedById,
            Action = action,
            Before = JsonConvert.SerializeObject(before),
            After = JsonConvert.SerializeObject(after)
        });
    }

    public static async Task CreateAuditTrail(this IAuditTrailsRepository auditRepository, IDateTimeService dateTime,
        AuditTableName tableName, Guid recordId, Guid changedById, AuditAction action,
        object? before = null, object? after = null)
    {
        before ??= new Dictionary<string, string>();
        after ??= new Dictionary<string, string>();

        await auditRepository.CreateAsync(new AuditTrailCreate
        {
            Timestamp = dateTime.NowDatabaseTime,
            TableName = tableName.ToString(),
            RecordId = recordId,
            ChangedBy = changedById,
            Action = action,
            Before = JsonConvert.SerializeObject(before),
            After = JsonConvert.SerializeObject(after)
        });
    }

    public static async Task CreateAuditTrail(this IAuditTrailsRepository auditRepository, IRunningServerState serverState, IDateTimeService dateTime,
        AuditTableName tableName, Guid recordId, AuditAction action, object? before = null, object? after = null)
    {
        await auditRepository.CreateAuditTrail(dateTime, tableName, recordId, serverState.SystemUserId, action, before, after);
    }
}