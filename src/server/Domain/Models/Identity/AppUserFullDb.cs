using Domain.DatabaseEntities.Identity;
using Domain.Enums.Identity;

namespace Domain.Models.Identity;

public class AppUserFullDb
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public bool PhoneNumberConfirmed { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid CreatedBy { get; set; }
    public string? ProfilePictureDataUrl { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public AccountType AccountType { get; set; } = AccountType.User;
    public string? Notes { get; set; }
    public List<AppRoleDb> Roles { get; set; } = new();
    public List<AppUserExtendedAttributeDb> ExtendedAttributes { get; set; } = new();
    public List<AppPermissionDb> Permissions { get; set; } = new();
    // Security Attributes
    public AuthState AuthState { get; set; }
}