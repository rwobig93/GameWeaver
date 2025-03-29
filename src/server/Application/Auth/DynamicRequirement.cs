using Domain.Enums.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Application.Auth;

public class DynamicRequirement : IAuthorizationRequirement
{
    public Guid EntityId { get; set; }
    public DynamicPermissionGroup Group { get; set; }
    public DynamicPermissionLevel Level { get; set; }
}