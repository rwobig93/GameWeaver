using FluentEmail.Core.Models;

namespace Application.Services.Integrations;

public interface IEmailService
{
    Task<SendResponse> SendRegistrationEmail(string emailAddress, string username, string confirmationUrl);
    Task<SendResponse> SendEmailChangeConfirmation(string emailAddress, string username, string confirmationUrl);
    Task<SendResponse> SendPasswordResetEmail(string emailAddress, string username, string confirmationUrl);
}