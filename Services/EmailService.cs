using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace EmailProvider.Services;
public class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailClient emailClient, ILogger<EmailService> logger)
    {
        _emailClient = emailClient;
        _logger = logger;
    }

    public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString());
            if (emailRequest != null)
            {
                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.UnpackEmailRequest() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
    {
        if (emailRequest.Subject != null && emailRequest.To != null)
        {
            try
            {
                _logger.LogInformation($"Sending email to {emailRequest.To} with subject {emailRequest.Subject}, HTMLBODY: {emailRequest.HtmlBody} och PLAINTEXT:{emailRequest.PlainText}");

                EmailSendOperation result = await _emailClient.SendAsync(
                    Azure.WaitUntil.Completed,
                    senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                    recipientAddress: emailRequest.To,
                    subject: emailRequest.Subject,
                    htmlContent: emailRequest.HtmlBody,
                    plainTextContent: emailRequest.PlainText);
                EmailSendResult statusMonitor = result.Value;

                _logger.LogInformation($"Email Sent. Status = {result.Value.Status}");
                string operationId = result.Id;
                _logger.LogInformation($"Email operation id = {operationId}");
            
            
                if (result.HasCompleted)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : EmailSender.SendEmail() :: {ex.Message}");
            }
        }

        return false;
    }
}
