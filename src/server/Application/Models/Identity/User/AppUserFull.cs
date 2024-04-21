using Application.Models.Identity.Permission;
using Application.Models.Identity.Role;
using Application.Models.Identity.UserExtensions;
using Domain.Enums.Identity;

namespace Application.Models.Identity.User;

public class AppUserFull
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string? EmailAddress { get; set; }
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
    // Entities from other tables
    public List<AppRoleSlim> Roles { get; set; } = [];
    public List<AppUserExtendedAttributeSlim> ExtendedAttributes { get; set; } = [];
    public List<AppPermissionSlim> Permissions { get; set; } = [];
    // Security Attributes
    public AuthState AuthState { get; set; }
}