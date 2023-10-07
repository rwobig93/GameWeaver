using System.ComponentModel.DataAnnotations;

namespace Application.Requests.v1.Identity.User;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}