using Azure.Messaging.ServiceBus;
using EmailProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace EmailProvider.Functions;

public class EmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IEmailService _emailService;
    public EmailSender(ILogger<EmailSender> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }
    [Function(nameof(EmailSender))]
    public async Task Run(
       [ServiceBusTrigger("email", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            var emailRequest = _emailService.UnpackEmailRequest(message);
            if (emailRequest != null && !string.IsNullOrEmpty(emailRequest.To))
            {
                var result = await _emailService.SendEmailAsync(emailRequest);
                if (result)
                {
                    await messageActions.CompleteMessageAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.Run() :: {ex.Message}");
        }
    }
}

