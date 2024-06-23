using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.LocalResource;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;

namespace Application.Mappers.GameServer;

public static class LocalResourceMappers
{
    public static LocalResourceSlim ToSlim(this LocalResourceDb localResourceDb)
    {
        return new LocalResourceSlim
        {
            Id = localResourceDb.Id,
            GameProfileId = localResourceDb.GameProfileId,
            Name = localResourceDb.Name,
            PathWindows = localResourceDb.PathWindows,
            PathLinux = localResourceDb.PathLinux,
            PathMac = localResourceDb.PathMac,
            Startup = localResourceDb.Startup,
            StartupPriority = localResourceDb.StartupPriority,
            Type = localResourceDb.Type,
            ContentType = localResourceDb.ContentType,
            Args = localResourceDb.Args,
            CreatedBy = localResourceDb.CreatedBy,
            CreatedOn = localResourceDb.CreatedOn,
            LastModifiedBy = localResourceDb.LastModifiedBy,
            LastModifiedOn = localResourceDb.LastModifiedOn,
            ConfigSets = []
        };
    }
    
    public static IEnumerable<LocalResourceSlim> ToSlims(this IEnumerable<LocalResourceDb> localResourceDbs)
    {
        return localResourceDbs.Select(ToSlim);
    }
    
    public static LocalResourceUpdate ToUpdate(this LocalResourceDb localResource)
    {
        return new LocalResourceUpdate
        {
            Id = localResource.Id,
            GameProfileId = localResource.GameProfileId,
            Name = localResource.Name,
            PathWindows = localResource.PathWindows,
            PathLinux = localResource.PathLinux,
            PathMac = localResource.PathMac,
            Startup = localResource.Startup,
            StartupPriority = localResource.StartupPriority,
            Type = localResource.Type,
            ContentType = localResource.ContentType,
            Args = localResource.Args,
            LastModifiedBy = localResource.LastModifiedBy,
            LastModifiedOn = localResource.LastModifiedOn
        };
    }

    public static LocalResourceHost ToHost(this LocalResourceDb localResource, Guid gameServerId, OsType osType)
    {
        return new LocalResourceHost
        {
            GameserverId = gameServerId,
            Name = localResource.Name,
            Path = osType switch
            {
                OsType.Unknown => localResource.PathWindows,
                OsType.Windows => localResource.PathWindows,
                OsType.Linux => localResource.PathLinux,
                OsType.Mac => localResource.PathMac,
                _ => localResource.PathWindows
            },
            Startup = localResource.Startup,
            Type = localResource.Type,
            ContentType = localResource.ContentType,
            Args = localResource.Args,
            ConfigSets = []
        };
    }

    public static IEnumerable<LocalResourceHost> ToHosts(this IEnumerable<LocalResourceDb> localResourceDbs, Guid gameServerId, OsType osType)
    {
        return localResourceDbs.Select(x => x.ToHost(gameServerId, osType));
    }

    public static LocalResourceHost ToHost(this LocalResourceSlim localResource, Guid gameServerId, OsType osType)
    {
        return new LocalResourceHost
        {
            GameserverId = gameServerId,
            Name = localResource.Name,
            Path = osType switch
            {
                OsType.Unknown => localResource.PathWindows,
                OsType.Windows => localResource.PathWindows,
                OsType.Linux => localResource.PathLinux,
                OsType.Mac => localResource.PathMac,
                _ => localResource.PathWindows
            },
            Startup = localResource.Startup,
            StartupPriority = localResource.StartupPriority,
            Type = localResource.Type,
            ContentType = localResource.ContentType,
            Args = localResource.Args,
            ConfigSets = new SerializableList<ConfigurationItemHost>(localResource.ConfigSets.ToHosts()),
            Id = localResource.Id
        };
    }

    public static IEnumerable<LocalResourceHost> ToHosts(this IEnumerable<LocalResourceSlim> localResources, Guid gameServerId, OsType osType)
    {
        return localResources.Select(x => x.ToHost(gameServerId, osType));
    }

    public static LocalResourceCreate ToCreate(this LocalResourceSlim localResource)
    {
        return new LocalResourceCreate
        {
            GameProfileId = localResource.GameProfileId,
            Name = localResource.Name,
            PathWindows = localResource.PathWindows,
            PathLinux = localResource.PathLinux,
            PathMac = localResource.PathMac,
            Startup = localResource.Startup,
            StartupPriority = localResource.StartupPriority,
            Type = localResource.Type,
            ContentType = localResource.ContentType,
            Args = localResource.Args,
            CreatedBy = localResource.CreatedBy,
            CreatedOn = localResource.CreatedOn,
            LastModifiedBy = localResource.LastModifiedBy,
            LastModifiedOn = localResource.LastModifiedOn
        };
    }
}