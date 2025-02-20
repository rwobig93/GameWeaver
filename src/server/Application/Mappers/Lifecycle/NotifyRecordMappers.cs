using Application.Models.Lifecycle;
using Domain.DatabaseEntities.Lifecycle;

namespace Application.Mappers.Lifecycle;

public static class NotifyRecordMappers
{
    public static NotifyRecordSlim ToSlim(this NotifyRecordDb record)
    {
        return new NotifyRecordSlim
        {
            Id = record.Id,
            EntityId = record.EntityId,
            Timestamp = record.Timestamp,
            Message = record.Message,
            Detail = record.Detail
        };
    }

    public static IEnumerable<NotifyRecordSlim> ToSlims(this IEnumerable<NotifyRecordDb> records)
    {
        return records.Select(ToSlim);
    }
}