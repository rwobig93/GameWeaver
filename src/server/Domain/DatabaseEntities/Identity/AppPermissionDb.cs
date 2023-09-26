using Domain.Contracts;

namespace Domain.DatabaseEntities.Identity;

public class AppPermissionDb : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = new(Guid.Empty.ToByteArray());
    public Guid RoleId { get; set; } = new (Guid.Empty.ToByteArray());
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
    public string Access { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}