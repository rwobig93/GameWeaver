using Application.Models.GameServer.LocalResource;
using Domain.Enums.GameServer;

namespace Application.Helpers.GameServer;

public static class LocalResourceHelpers
{
    public static void MergeResources(this List<LocalResourceSlim> existing, List<LocalResourceSlim> priority)
    {
        foreach (var resource in priority)
        {
            var matchingResource = existing.FirstOrDefault(x =>
                x.Type == resource.Type &&
                x.Args == resource.Args &&
                ((x.PathWindows.Length > 0 && x.PathWindows == resource.PathWindows) ||
                 (x.PathLinux.Length > 0 && x.PathLinux == resource.PathLinux) ||
                 (x.PathMac.Length > 0 && x.PathMac == resource.PathMac)));

            if (matchingResource is null)
            {
                existing.Add(resource);
                continue;
            }

            if (resource.ContentType == ContentType.Ignore)
            {
                existing.Remove(matchingResource);
                continue;
            }

            matchingResource.Id = resource.Id;
            matchingResource.GameProfileId = resource.GameProfileId;
            matchingResource.Startup = resource.Startup;
            matchingResource.StartupPriority = resource.StartupPriority;
            matchingResource.ContentType = resource.ContentType;
            var updatedConfigSets = matchingResource.ConfigSets.ToList();

            foreach (var config in resource.ConfigSets)
            {
                if (config.DuplicateKey)
                {
                    var matchingConfigDuplicate = updatedConfigSets.FirstOrDefault(x =>
                        x.Category == config.Category &&
                        x.Path == config.Path &&
                        x.Key == config.Key &&
                        x.Value == config.Value);

                    if (config.Ignore && matchingConfigDuplicate is not null)
                    {
                        updatedConfigSets.Remove(matchingConfigDuplicate);
                        continue;
                    }

                    if (matchingConfigDuplicate is not null)
                    {
                        continue;
                    }

                    updatedConfigSets.Add(config);
                    continue;
                }

                // Key is not a duplicate key
                var matchingConfig = updatedConfigSets.FirstOrDefault(x =>
                    x.Category == config.Category &&
                    x.Path == config.Path &&
                    x.Key == config.Key);

                if (config.Ignore && matchingConfig is not null)
                {
                    updatedConfigSets.Remove(matchingConfig);
                    continue;
                }

                if (matchingConfig is not null)
                {
                    matchingConfig.Id = config.Id;
                    matchingConfig.LocalResourceId = config.LocalResourceId;
                    matchingConfig.Value = config.Value;
                    continue;
                }

                updatedConfigSets.Add(config);
            }

            matchingResource.ConfigSets = updatedConfigSets;
        }
    }
}