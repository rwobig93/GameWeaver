using Application.Models.GameServer.ConfigResourceTreeItem;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.LocalResource;

namespace Application.Mappers.GameServer;

public static class GameServerConfigResourceTreeItemMappers
{
    public static LocalResourceSlim ToLocalResource(this ConfigResourceTreeItem item)
    {
        return new LocalResourceSlim
        {
            Id = item.Id,
            Name = item.Name
        };
    }

    public static ConfigurationItemSlim ToConfigurationItem(this ConfigResourceTreeItem item)
    {
        return new ConfigurationItemSlim
        {
            Id = item.Id,
            DuplicateKey = item.DuplicateKey,
            Ignore = item.Ignore,
            Path = item.Path,
            Category = item.Category,
            Key = item.Key,
            Value = item.Value,
            FriendlyName = item.Name
        };
    }
}