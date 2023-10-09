using Application.Models.GameServer.ConfigurationItem;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class ConfigurationItemMappers
{
    public static ConfigurationItemSlim ToSlim(this ConfigurationItemDb configDb)
    {
        return new ConfigurationItemSlim
        {
            Id = configDb.Id,
            GameProfileId = configDb.GameProfileId,
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
            Path = configDb.Path,
            Category = configDb.Category,
            Key = configDb.Key,
            Value = configDb.Value
        };
    }
}