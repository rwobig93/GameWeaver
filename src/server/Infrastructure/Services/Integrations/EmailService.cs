using Application.Constants.Communication;
using Application.Models.Email;
using Application.Services.Integrations;
using FluentEmail.Core;
using FluentEmail.Core.Models;

namespace Infrastructure.Services.Integrations;

public class EmailService : IEmailService
{
    private readonly IFluentEmail _mailService;
    private readonly ILogger _logger;

    public EmailService(IFluentEmail mailService, ILogger logger)
    {
        _mailService = mailService;
        _logger = logger;
    }

    public async Task<SendResponse> SendRegistrationEmail(string emailAddress, string username, string confirmationUrl)
    {
        try
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), EmailConstants.TemplatesPath,
                EmailConstants.PathRegistrationConfirmation);
        
            var sendResponse = await _mailService.Subject("Registration Confirmation").To(emailAddress)
                .UsingTemplateFromFile(templatePath, new EmailAction() 
                    {ActionUrl = confirmationUrl, Username = username})
                .SendAsync();
            if (!sendResponse.Successful)
            {
                throw new Exception(sendResponse.ErrorMessages.First());
            }

            _logger.Debug("Successfully sent registration email! => {Email} | {Username} | {Url}",
                emailAddress, username, confirmationUrl);
            return sendResponse;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send registration email => {Email} | {Username} | {Url}", 
                emailAddress, username, confirmationUrl);
            return new SendResponse
            {
                MessageId = "Error",
                ErrorMessages = new List<string> { ex.Message }
            };
        }
    }
    
    public async Task<SendResponse> SendEmailChangeConfirmation(string emailAddress, string username, string confirmationUrl)
    {
        try
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), EmailConstants.TemplatesPath,
                EmailConstants.PathEmailChangeConfirmation);
        
            var sendResponse = await _mailService.Subject("Email Change Confirmation").To(emailAddress)
                .UsingTemplateFromFile(templatePath, new EmailAction() 
                    {ActionUrl = confirmationUrl, Username = username})
                .SendAsync();
            if (!sendResponse.Successful)
            {
                throw new Exception(sendResponse.ErrorMessages.First());
            }

            _logger.Debug("Successfully sent email address change email! => {Email} | {Username} | {Url}",
                emailAddress, username, confirmationUrl);
            return sendResponse;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send email address change email => {Email} | {Username} | {Url}", 
                emailAddress, username, confirmationUrl);
            return new SendResponse
            {
                MessageId = "Error",
                ErrorMessages = new List<string> { ex.Message }
            };
        }
    }
    
    public async Task<SendResponse> SendPasswordResetEmail(string emailAddress, string username, string confirmationUrl)
    {
        try
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), EmailConstants.TemplatesPath,
                EmailConstants.PathPasswordReset);
        
            var sendResponse = await _mailService.Subject("Password Reset Confirmation").To(emailAddress)
                .UsingTemplateFromFile(templatePath, new EmailAction() 
                    {ActionUrl = confirmationUrl, Username = username})
                .SendAsync();
            if (!sendResponse.Successful)
            {
                throw new Exception(sendResponse.ErrorMessages.First());
            }

            _logger.Debug("Successfully sent password reset email! => {Email} | {Username} | {Url}",
                emailAddress, username, confirmationUrl);
            return sendResponse;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send password reset email => {Email} | {Username} | {Url}", 
                emailAddress, username, confirmationUrl);
            return new SendResponse
            {
                MessageId = "Error",
                ErrorMessages = new List<string> { ex.Message }
            };
        }
    }
}