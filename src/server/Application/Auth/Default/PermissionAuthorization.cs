using Microsoft.AspNetCore.Authorization;

namespace Application.Auth.Default;

public class PermissionAuthorization : IAuthorizationRequirement
{
    public string Permission { get; private set; }

    public PermissionAuthorization(string permission)
    {
        Permission = permission;
    }
}
