using Application.Models.Identity.External;
using OAuth2.Models;

namespace Application.Mappers.Integrations;

public static class ExternalAuthMappers
{
    public static ExternalUserProfile ToExternalProfile(this UserInfo? userProfile)
    {
        if (userProfile is null)
            return new ExternalUserProfile();

        return new ExternalUserProfile
        {
            Id = userProfile.Id,
            Email = userProfile.Email,
            AvatarUri = userProfile.PhotoUri
        };
    }
}