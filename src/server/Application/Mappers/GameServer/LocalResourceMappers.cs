using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.LocalResource;
using Domain.Contracts;
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
            ContentType = localResourceDb.ContentType,
            Extension = localResourceDb.Extension,
            Args = localResourceDb.Args,
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
            GameServerId = localResource.GameServerId,
            Name = localResource.Name,
            Path = localResource.Path,
            Startup = localResource.Startup,
            StartupPriority = localResource.StartupPriority,
            Type = localResource.Type,
            ContentType = localResource.ContentType,
            Extension = localResource.Extension,
            Args = localResource.Args
        };
    }

    public static LocalResourceHost ToHost(this LocalResourceDb localResource)
    {
        return new LocalResourceHost
        {
            GameserverId = localResource.GameServerId,
            Name = localResource.Name,
            Path = localResource.Path,
            Startup = localResource.Startup,
            Type = localResource.Type,
            ContentType = localResource.ContentType,
            Extension = localResource.Extension,
            Args = localResource.Args,
            ConfigSets = []
        };
    }

    public static IEnumerable<LocalResourceHost> ToHosts(this IEnumerable<LocalResourceDb> localResourceDbs)
    {
        return localResourceDbs.Select(ToHost);
    }

    public static LocalResourceHost ToHost(this LocalResourceSlim localResource)
    {
        return new LocalResourceHost
        {
            GameserverId = localResource.GameServerId,
            Name = localResource.Name,
            Path = localResource.Path,
            Startup = localResource.Startup,
            StartupPriority = localResource.StartupPriority,
            Type = localResource.Type,
            ContentType = localResource.ContentType,
            Extension = localResource.Extension,
            Args = localResource.Args,
            ConfigSets = new SerializableList<ConfigurationItemHost>(localResource.ConfigSets.ToHosts()),
            Id = localResource.Id
        };
    }

    public static IEnumerable<LocalResourceHost> ToHosts(this IEnumerable<LocalResourceSlim> localResources)
    {
        return localResources.Select(ToHost);
    }
}