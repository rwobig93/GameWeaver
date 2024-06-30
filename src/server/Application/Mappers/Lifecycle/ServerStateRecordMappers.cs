using Application.Models.Lifecycle;
using Domain.DatabaseEntities.Lifecycle;

namespace Application.Mappers.Lifecycle;

public static class ServerStateRecordMappers
{
    public static ServerStateRecordCreate ToCreate(this ServerStateRecordDb stateRecord)
    {
        return new ServerStateRecordCreate
        {
            Timestamp = DateTime.Now,
            AppVersion = stateRecord.AppVersion,
            DatabaseVersion = stateRecord.DatabaseVersion
        };
    }
}