using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.LocalResource;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;

namespace Application.Mappers.GameServer;

public static class ConfigurationItemMappers
{
    public static ConfigurationItemSlim ToSlim(this ConfigurationItemDb configDb)
    {
        return new ConfigurationItemSlim
        {
            Id = configDb.Id,
            GameProfileId = configDb.GameProfileId,
            DuplicateKey = configDb.DuplicateKey,
            Path = configDb.Path,
            Category = configDb.Category,
            Key = configDb.Key,
            Value = configDb.Value
        };
    }
    
    public static IEnumerable<ConfigurationItemSlim> ToSlims(this IEnumerable<ConfigurationItemDb> configDbs)
    {
        return configDbs.Select(x => x.ToSlim()).ToList();
    }
    
    public static ConfigurationItemUpdate ToUpdate(this ConfigurationItemDb configDb)
    {
        return new ConfigurationItemUpdate
        {
            Id = configDb.Id,
            GameProfileId = configDb.GameProfileId,
            DuplicateKey = configDb.DuplicateKey,
            Path = configDb.Path,
            Category = configDb.Category,
            Key = configDb.Key,
            Value = configDb.Value
        };
    }
    
    public static ConfigurationItemCreate ToCreate(this ConfigurationItemDb configDb)
    {
        return new ConfigurationItemCreate
        {
            GameProfileId = configDb.GameProfileId,
            DuplicateKey = configDb.DuplicateKey,
            Path = configDb.Path,
            Category = configDb.Category,
            Key = configDb.Key,
            Value = configDb.Value
        };
    }

    public static ConfigurationItemHost ToHost(this ConfigurationItemDb configDb)
    {
        return new ConfigurationItemHost
        {
            Id = configDb.Id,
            DuplicateKey = configDb.DuplicateKey,
            Category = configDb.Category,
            Key = configDb.Key,
            Value = configDb.Value
        };
    }

    public static IEnumerable<ConfigurationItemHost> ToHosts(this IEnumerable<ConfigurationItemDb> configDbs)
    {
        return configDbs.Select(x => x.ToHost()).ToList();
    }

    public static LocalResourceHost ToHostResource(this ConfigurationItemDb configDb)
    {
        return new LocalResourceHost
        {
            GameserverId = new Guid(),
            Path = configDb.Path,
            Startup = false,
            Extension = Path.GetExtension(configDb.Path),
            Type = ResourceType.ConfigFile,
            ConfigSets = [configDb.ToHost()]
        };
    }
}