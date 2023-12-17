using Application.Models.GameServer.LocalResource;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class LocalResourceMappers
{
    public static LocalResourceSlim ToSlim(this LocalResourceDb localResourceDb)
    {
        return new LocalResourceSlim
        {
            Id = localResourceDb.Id,
            GameProfileId = localResourceDb.GameProfileId,
            GameServerId = localResourceDb.GameServerId,
            Name = localResourceDb.Name,
            Path = localResourceDb.Path,
            Startup = localResourceDb.Startup,
            StartupPriority = localResourceDb.StartupPriority,
            Type = localResourceDb.Type,
            Extension = localResourceDb.Extension,
            Args = localResourceDb.Args
        };
    }
    
    public static IEnumerable<LocalResourceSlim> ToSlims(this IEnumerable<LocalResourceDb> localResourceDbs)
    {
        return localResourceDbs.Select(ToSlim);
    }
    
    public static LocalResourceUpdate ToUpdate(this LocalResourceDb localResourceDb)
    {
        return new LocalResourceUpdate
        {
            Id = localResourceDb.Id,
            GameProfileId = localResourceDb.GameProfileId,
            GameServerId = localResourceDb.GameServerId,
            Name = localResourceDb.Name,
            Path = localResourceDb.Path,
            Startup = localResourceDb.Startup,
            StartupPriority = localResourceDb.StartupPriority,
            Type = localResourceDb.Type,
            Extension = localResourceDb.Extension,
            Args = localResourceDb.Args
        };
    }
}