using Application.Helpers.Runtime;

namespace Application.Models.Identity.Permission;

public class AppPermissionCreate
{
    public Guid RoleId { get; set; } = GuidHelpers.GetMax();
    public Guid UserId { get; set; } = GuidHelpers.GetMax();
    public string ClaimType { get; set; } = ApplicationClaimTypes.Permission;
    public string ClaimValue { get; set; } = "";
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
    public string Access { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}