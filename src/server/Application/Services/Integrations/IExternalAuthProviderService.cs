using Application.Models.Identity.External;
using Domain.Contracts;
using Domain.Enums.Integration;

namespace Application.Services.Integrations;

public interface IExternalAuthProviderService
{
    public bool AnyProvidersEnabled { get; }
    public bool ProviderEnabledGoogle { get; }
    public bool ProviderEnabledDiscord { get; }
    public bool ProviderEnabledSpotify { get; }
    
    public Task<IResult<string>> GetLoginUri(ExternalAuthProvider provider, ExternalAuthRedirect redirect);
    public Task<IResult<ExternalUserProfile>> GetUserProfile(ExternalAuthProvider provider, string oauthCode);
}