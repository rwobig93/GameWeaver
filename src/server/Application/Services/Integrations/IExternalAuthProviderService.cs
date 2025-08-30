using Application.Models.Identity.External;
using Domain.Contracts;
using Domain.Enums.Integrations;

namespace Application.Services.Integrations;

public interface IExternalAuthProviderService
{
    public bool AnyProvidersEnabled { get; }
    public bool ProviderEnabledGoogle { get; }
    public bool ProviderEnabledDiscord { get; }
    public bool ProviderEnabledSpotify { get; }
    public bool ProviderEnabledCustomOne { get; }
    public bool ProviderEnabledCustomTwo { get; }
    public bool ProviderEnabledCustomThree { get; }

    public Task<IResult<string>> GetLoginUri(ExternalAuthProvider provider, ExternalAuthRedirect redirect);
    public Task<IResult<ExternalUserProfile>> GetUserProfile(ExternalAuthProvider provider, string oauthCode);
}