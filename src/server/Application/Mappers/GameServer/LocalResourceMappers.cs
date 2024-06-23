using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.LocalResource;
using Application.Requests.GameServer.LocalResource;
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

    public static LocalResourceCreate ToCreate(this LocalResourceSlim resource)
    {
        return new LocalResourceCreate
        {
            GameProfileId = resource.GameProfileId,
            Name = resource.Name,
            PathWindows = resource.PathWindows,
            PathLinux = resource.PathLinux,
            PathMac = resource.PathMac,
            Startup = resource.Startup,
            StartupPriority = resource.StartupPriority,
            Type = resource.Type,
            ContentType = resource.ContentType,
            Args = resource.Args,
            CreatedBy = resource.CreatedBy,
            CreatedOn = resource.CreatedOn,
            LastModifiedBy = resource.LastModifiedBy,
            LastModifiedOn = resource.LastModifiedOn
        };
    }

    public static LocalResourceCreate ToCreate(this LocalResourceCreateRequest request)
    {
        return new LocalResourceCreate
        {
            GameProfileId = request.GameProfileId,
            Name = request.Name,
            PathWindows = request.PathWindows,
            PathLinux = request.PathLinux,
            PathMac = request.PathMac,
            Startup = request.Startup,
            StartupPriority = request.StartupPriority,
            Type = request.Type,
            ContentType = request.ContentType,
            Args = request.Args
        };
    }

    public static LocalResourceUpdate ToUpdate(this LocalResourceUpdateRequest request)
    {
        return new LocalResourceUpdate
        {
            Id = request.Id,
            Name = request.Name,
            PathWindows = request.PathWindows,
            PathLinux = request.PathLinux,
            PathMac = request.PathMac,
            Startup = request.Startup,
            StartupPriority = request.StartupPriority,
            Type = request.Type,
            ContentType = request.ContentType,
            Args = request.Args
        };
    }

    public static LocalResourceCreateRequest ToCreateRequest(this LocalResourceSlim resource)
    {
        return new LocalResourceCreateRequest()
        {
            GameProfileId = resource.GameProfileId,
            Name = resource.Name,
            PathWindows = resource.PathWindows,
            PathLinux = resource.PathLinux,
            PathMac = resource.PathMac,
            Startup = resource.Startup,
            StartupPriority = resource.StartupPriority,
            Type = resource.Type,
            ContentType = resource.ContentType,
            Args = resource.Args
        };
    }
}