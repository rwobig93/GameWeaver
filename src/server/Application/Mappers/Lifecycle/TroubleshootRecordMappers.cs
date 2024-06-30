using Application.Models.Lifecycle;
using Domain.DatabaseEntities.Lifecycle;
using Newtonsoft.Json;

namespace Application.Mappers.Lifecycle;

public static class TroubleshootRecordMappers
{
    public static TroubleshootingRecordSlim ToSlim(this TroubleshootingRecordDb record)
    {
        return new TroubleshootingRecordSlim
        {
            Id = record.Id,
            EntityType = record.EntityType,
            RecordId = record.RecordId,
            ChangedBy = record.ChangedBy,
            Timestamp = record.Timestamp,
            Message = record.Message,
            Detail = record.Detail is null ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(record.Detail)
        };
    }

    public static IEnumerable<TroubleshootingRecordSlim> ToSlims(this IEnumerable<TroubleshootingRecordDb> records)
    {
        return records.Select(ToSlim);
    }
}