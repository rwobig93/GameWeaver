using Application.Models.GameServer.ConfigurationItem;
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
                x.Path == resource.Path &&
                x.Type == resource.Type &&
                x.Extension == resource.Extension &&
                x.Args == resource.Args);
            
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

            matchingResource.Startup = resource.Startup;
            matchingResource.StartupPriority = resource.StartupPriority;
            matchingResource.ContentType = resource.ContentType;
            var updatedConfigSets = matchingResource.ConfigSets.ToList(); 

            foreach (var config in resource.ConfigSets)
            {
                var matchingConfigs = updatedConfigSets.Where(x =>
                    x.Category == config.Category &&
                    x.Path == config.Path &&
                    x.Key == config.Key).ToList();

                if (matchingConfigs.Count == 0)
                {
                    updatedConfigSets.Add(config);
                    continue;
                }

                if (!config.DuplicateKey)
                {
                    var matchingConfig = matchingConfigs.First();
                    matchingConfig.Value = config.Value;
                    continue;
                }

                if (config.Ignore)
                {
                    continue;
                }
                
                
            }
        }
    }
}