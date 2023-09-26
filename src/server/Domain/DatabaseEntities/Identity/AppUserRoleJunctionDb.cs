namespace Domain.DatabaseEntities.Identity;

public class AppUserRoleJunctionDb
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}