using Domain.Enums.Identity;

namespace Application.Models.Identity.User;

public class AppUserUpdate
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? PhoneNumberConfirmed { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureDataUrl { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public AccountType AccountType { get; set; } = AccountType.User;
    public string? Notes { get; set; }
    public int? Currency { get; set; }
}