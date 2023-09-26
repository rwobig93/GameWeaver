using Application.Models.Lifecycle;
using Application.Responses.Lifecycle;
using Domain.DatabaseEntities.Lifecycle;

namespace Application.Mappers.Lifecycle;

public static class AuditMappers
{
    public static AuditTrailCreate ToCreate(this AuditTrailDb auditTrailDb)
    {
        return new AuditTrailCreate
        {
            TableName = auditTrailDb.TableName,
            RecordId = auditTrailDb.RecordId,
            ChangedBy = auditTrailDb.ChangedBy,
            Action = auditTrailDb.Action,
            Timestamp = auditTrailDb.Timestamp,
            Before = auditTrailDb.Before ?? "",
            After = auditTrailDb.After
        };
    }

    public static AuditTrailDb ToDb(this AuditTrailCreate auditTrailCreate)
    {
        return new AuditTrailDb
        {
            TableName = auditTrailCreate.TableName,
            RecordId = auditTrailCreate.RecordId,
            ChangedBy = auditTrailCreate.ChangedBy,
            Action = auditTrailCreate.Action,
            Timestamp = auditTrailCreate.Timestamp,
            Before = auditTrailCreate.Before,
            After = auditTrailCreate.After
        };
    }
    
    public static AuditTrailSlim ToSlim(this AuditTrailWithUserDb auditTrailDb)
    {
        return new AuditTrailSlim
        {
            Id = auditTrailDb.Id,
            TableName = auditTrailDb.TableName,
            RecordId = auditTrailDb.RecordId,
            ChangedBy = auditTrailDb.ChangedBy,
            ChangedByUsername = auditTrailDb.ChangedByUsername,
            Timestamp = auditTrailDb.Timestamp,
            Action = auditTrailDb.Action,
            Before = new Dictionary<string, string>(),
            After = new Dictionary<string, string>()
        };
    }

    public static IEnumerable<AuditTrailSlim> ToSlims(this IEnumerable<AuditTrailWithUserDb> auditTrailDbs)
    {
        return auditTrailDbs.Select(x => x.ToSlim());
    }

    public static AuditTrailResponse ToResponse(this AuditTrailSlim auditTrailSlim)
    {
        return new AuditTrailResponse
        {
            Id = auditTrailSlim.Id,
            TableName = auditTrailSlim.TableName,
            RecordId = auditTrailSlim.RecordId,
            ChangedBy = auditTrailSlim.ChangedBy,
            ChangedByUsername = auditTrailSlim.ChangedByUsername,
            Timestamp = auditTrailSlim.Timestamp,
            Action = auditTrailSlim.Action.ToString(),
            Before = auditTrailSlim.Before,
            After = auditTrailSlim.After
        };
    }

    public static IEnumerable<AuditTrailResponse> ToResponses(this IEnumerable<AuditTrailSlim> auditTrailSlims)
    {
        return auditTrailSlims.Select(x => x.ToResponse());
    }
}