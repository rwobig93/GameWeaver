namespace Application.Models.Identity.Role;

public class AppRoleCreate
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}