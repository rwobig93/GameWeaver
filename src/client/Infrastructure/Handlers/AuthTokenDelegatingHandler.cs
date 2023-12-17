using System.Net.Http.Headers;
using Application.Constants;
using Application.Services;

namespace Infrastructure.Handlers;

public class AuthTokenDelegatingHandler : DelegatingHandler
{
    private readonly IControlServerService _serverService;
    private readonly ILogger _logger;

    public AuthTokenDelegatingHandler(IControlServerService serverService, ILogger logger)
    {
        _serverService = serverService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Ensure token isn't expired
        var tokenEnforcement = await _serverService.EnsureAuthTokenIsUpdated();
        if (!tokenEnforcement.Succeeded)
            _logger.Error("Failed to enforce authorization token: {ErrorMessage}", tokenEnforcement.Messages);
        
        // Add current valid token to the request headers
        request.Headers.Authorization =
            new AuthenticationHeaderValue(ApiConstants.AuthorizationScheme, _serverService.ActiveToken.Token);
        
        // Handle and return the response
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}