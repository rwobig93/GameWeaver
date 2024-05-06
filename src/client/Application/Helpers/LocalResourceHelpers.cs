using Domain.Models.GameServer;

namespace Application.Helpers;

public static class LocalResourceHelpers
{
    public static string GetFullPath(this LocalResource localResource)
    {
        var gameServerPath = Path.Join(OsHelpers.GetDefaultGameServerPath(), localResource.GameserverId.ToString());
        var localResourcePath = string.IsNullOrWhiteSpace(localResource.Extension) ? localResource.Path : $"{localResource.Path}.{localResource.Extension}";
        return Path.Join(gameServerPath, localResourcePath);
    }
}