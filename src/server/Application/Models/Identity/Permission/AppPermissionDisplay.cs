using Application.Helpers.Runtime;

namespace Application.Models.Identity.Permission;

public class AppPermissionDisplay
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = GuidHelpers.GetMax();
    public string UserName { get; set; } = string.Empty;
    public Guid RoleId { get; set; } = GuidHelpers.GetMax();
    public string RoleName { get; set; } = string.Empty;
    public string? ClaimType { get; set; } = "";
    public string ClaimValue { get; set; } = "";
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
    public string Access { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}