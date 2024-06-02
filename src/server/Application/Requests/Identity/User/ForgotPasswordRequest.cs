using System.ComponentModel.DataAnnotations;

namespace Application.Requests.Identity.User;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}