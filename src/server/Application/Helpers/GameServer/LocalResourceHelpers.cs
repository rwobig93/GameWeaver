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
                existing.Add(resource);
                continue;
            }

            matchingResource.Id = resource.Id;
            matchingResource.GameProfileId = resource.GameProfileId;
            matchingResource.Startup = resource.Startup;
            matchingResource.StartupPriority = resource.StartupPriority;
            matchingResource.ContentType = resource.ContentType;

            var existingConfigSets = matchingResource.ConfigSets.ToList();
            var mergedConfigSets = existingConfigSets.MergeConfiguration(resource.ConfigSets);

            matchingResource.ConfigSets = mergedConfigSets;
        }
    }
}