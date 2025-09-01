using Application.Constants.GameServer;
using Application.Models.GameServer.GameProfile;
using Application.Requests.GameServer.GameProfile;

namespace Application.Helpers.GameServer;

public static class GameProfileHelpers
{
    public static bool HasInvalidName(this GameProfileSlim profile)
    {
        return GameProfileConstants.InvalidProfileNames.Any(invalidName => profile.FriendlyName.StartsWith(invalidName, StringComparison.InvariantCultureIgnoreCase));
    }

    public static bool HasInvalidName(this GameProfileExport profile)
    {
        return GameProfileConstants.InvalidProfileNames.Any(invalidName => profile.Name.StartsWith(invalidName, StringComparison.InvariantCultureIgnoreCase));
    }

    public static bool HasInvalidName(this GameProfileCreateRequest profile)
    {
        return GameProfileConstants.InvalidProfileNames.Any(invalidName => profile.Name.StartsWith(invalidName, StringComparison.InvariantCultureIgnoreCase));
    }

    public static bool HasInvalidName(this GameProfileCreate profile)
    {
        return GameProfileConstants.InvalidProfileNames.Any(invalidName => profile.FriendlyName.StartsWith(invalidName, StringComparison.InvariantCultureIgnoreCase));
    }

    public static bool HasInvalidName(this GameProfileUpdateRequest profile)
    {
        if (profile.Name is null)
        {
            return false;
        }

        return GameProfileConstants.InvalidProfileNames.Any(invalidName => profile.Name.StartsWith(invalidName, StringComparison.InvariantCultureIgnoreCase));
    }
}