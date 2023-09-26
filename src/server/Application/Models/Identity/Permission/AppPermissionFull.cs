using Application.Helpers.Runtime;
using Application.Models.Identity.Role;
using Application.Models.Identity.User;

namespace Application.Models.Identity.Permission;

public class AppPermissionFull
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = GuidHelpers.GetMax();
    public Guid RoleId { get; set; } = GuidHelpers.GetMax();
    public string ClaimType { get; set; } = "";
    public string ClaimValue { get; set; } = "";
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
    public string Access { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public List<AppUserSlim> Users { get; set; } = new();
    public List<AppRoleSlim> Roles { get; set; } = new();
}