using System.ComponentModel.DataAnnotations;

namespace Application.Requests.Identity.User;

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = null!;

    [Required]
    public string RequestCode { get; set; } = null!;
}