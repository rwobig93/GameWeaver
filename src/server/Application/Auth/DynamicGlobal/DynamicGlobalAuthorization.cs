using Domain.Enums.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Application.Auth.DynamicGlobal;

public class DynamicGlobalAuthorization : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public DynamicGlobalAuthorization(string globalPermission, DynamicPermissionGroup group, DynamicPermissionLevel level)
    {
        EntityId = Guid.Empty;
        Group = group;
        Level = level;
        GlobalPermission = globalPermission;
    }

    public DynamicGlobalAuthorization(string globalPermission, Guid entityId, DynamicPermissionGroup group, DynamicPermissionLevel level)
    {
        EntityId = entityId;
        Group = group;
        Level = level;
        GlobalPermission = globalPermission;
    }

    public string GlobalPermission { get; set; }
    public Guid EntityId { get; set; }
    public DynamicPermissionGroup Group { get; set; }
    public DynamicPermissionLevel Level { get; set; }
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}