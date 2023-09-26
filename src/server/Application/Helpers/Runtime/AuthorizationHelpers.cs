using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Application.Helpers.Runtime;

public static class AuthorizationHelpers
{
    public static async Task<bool> UserHasPermission(this IAuthorizationService authService, ClaimsPrincipal? currentUser, string permission)
    {
        return (await authService.AuthorizeAsync(currentUser!, null, permission)).Succeeded;
    }
}